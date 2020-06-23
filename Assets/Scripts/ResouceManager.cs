using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELoadResPriority
{
    RES_HIGHT = 0,//最高优先级
    RES_MIDDLE,//一般优先级
    RES_SLOW,//低优先级
    RES_NUM,
}

public class AsyncLoadResParam
{
    public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
    public uint m_Crc;
    public string m_Path;
    public bool m_Sprite = false;
    public ELoadResPriority m_Priority = ELoadResPriority.RES_SLOW;

    public void Reset()
    {
        m_CallBackList.Clear();
        m_Crc = 0;
        m_Path = "";
        m_Sprite = false;
        m_Priority = ELoadResPriority.RES_SLOW;
    }
}

public class AsyncCallBack
{
    //加载完成得回调
    public OnAsyncObjFinish m_DealFinish = null;
    //回调参数
    public object m_Param1 = null, m_Param2 = null, m_Param3 = null;

    public void Reset()
    {
        m_DealFinish = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
    }
}

public delegate void OnAsyncObjFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);

public class ResourceManager : Singleton<ResourceManager>
{
    public bool m_LoadFromAssetBundle = true;
    //缓存使用的资源列表 
    public Dictionary<uint, ResourceItem> AssetDic = new Dictionary<uint, ResourceItem>();
    //缓存应用为零的资源列表，达到缓存最大的时 释放这个列表里面最早没用的资源
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

    //中间类 回调类 的类对象池
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);

    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);

    //Mono脚本
    protected MonoBehaviour m_SatrtMono;
    //正在异步加载的资源列表
    protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)ELoadResPriority.RES_NUM];

    //正在异步加载得dic
    protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();

    //最长连续卡着加载资源的时间 单位微秒
    private const long MAXLOADRESTIME = 200000;

    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)ELoadResPriority.RES_NUM; i++)
        {
            m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }
        m_SatrtMono = mono;
        m_SatrtMono.StartCoroutine(AsyncLoadCor());
    }

    /// <summary>
    /// 清空缓存 一般用于跳场景
    /// </summary>
    public void ClearCache()
    {
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach (ResourceItem item in AssetDic.Values)
        {
            if (item.m_Clear)
            {
                tempList.Add(item);
            }
        }

        foreach (ResourceItem item in tempList)
        {
            DestroyResourceItem(item, item.m_Clear);
        }
        tempList.Clear();
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="path"></param>
    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResouceItem(crc);
        if (item != null)
        {
            return;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResouceItem(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj;
            }
            else
            {
                obj = LoadAssetByEditor<Object>(path);
            }

        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        CacheResouce(path, ref item, crc, obj);

        //跳场景不清空缓存
        item.m_Clear = false;
        ReleaseResource(obj, false);
    }

    /// <summary>
    /// 同步资源加载 外部直接调用 仅加载不需要实例化的资源 例如teture 音频等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResouce<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResouceItem(crc);
        if (item != null)
        {
            return item.m_Obj as T;
        }

        T obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResouceItem(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj as T;
            }
            else
            {
                obj = LoadAssetByEditor<T>(path);
            }

        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }

        CacheResouce(path, ref item, crc, obj);

        return obj;
    }

    /// <summary>
    /// 不需要实例化的资源卸载，根据对象
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(Object obj, bool destroyObj = false)
    {
        if (obj == null)
        {
            return false;
        }

        ResourceItem item = null;
        foreach (ResourceItem res in AssetDic.Values)
        {
            if (res.m_Guid == obj.GetInstanceID())
            {
                item = res;
            }
        }

        if (item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源：" + obj.name + " 可能释放了多次");
            return false;
        }
        item.RefCount--;
        DestroyResourceItem(item, destroyObj);
        return true;
    }

    /// <summary>
    /// 不需要实例化的资源卸载，根据路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(string path, bool destroyObj = false)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError(" AssetDic 里不存在该资源 ：" + path + " 可 能 释 放 了 多");
        }

        item.RefCount--;
        DestroyResourceItem(item, destroyObj);
        return true;
    }

    /// <summary>
    /// 缓存加载的资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    /// <param name="addrefcount"></param>
    void CacheResouce(string path, ref ResourceItem item, uint crc, Object obj, int addrefcount = 1)
    {
        //缓存太多 清除最早没有使用的资源
        WashOut();

        if (item == null)
        {
            Debug.LogError("ResouceLoad is null , path : " + path);
        }
        if (obj == null)
        {
            Debug.LogError("ResouceLoad Fail : " + path);
        }
        item.m_Obj = obj;
        item.m_Guid = obj.GetInstanceID();
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addrefcount;
        ResourceItem oldItem = null;
        if (AssetDic.TryGetValue(item.m_Crc, out oldItem))
        {
            AssetDic[item.m_Crc] = item;
        }
        else
        {
            AssetDic.Add(item.m_Crc, item);
        }
    }

    /// <summary>
    /// 缓存太多清除最早没有使用的资源
    /// </summary>
    protected void WashOut()
    {
        //当前内存使用大于百分之80 来进行清除最早没用的资源
        //{
        //    if (m_NoRefrenceAssetMapList.Size() <= 0)
        //    {
        //        break;
        //        ResouceItem item = m_NoRefrenceAssetMapList.Back();
        //        DestroyResouceItem(item,true);
        //        m_NoRefrenceAssetMapList.Pop);
        //    }
        //}
    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroy"></param>
    protected void DestroyResourceItem(ResourceItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0)
        {
            return;
        }

        if (!destroyCache)
        {
            //m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }

        if (!AssetDic.Remove(item.m_Crc))
        {
            return;
        }

        //释放assetbundle引用
        AssetBundleManager.Instance.ReleaseAsset(item);
        if (item.m_Obj != null)
        {
            item.m_Obj = null;
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#endif
        }
    }

#if UNITY_EDITOR
    protected T LoadAssetByEditor<T>(string path) where T : Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    ResourceItem GetCacheResouceItem(uint crc, int addrefcount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item))
        {
            if (item != null)
            {
                item.RefCount += addrefcount;
                item.m_LastUseTime = Time.realtimeSinceStartup;

                //if (item.RefCount<=1)
                //{
                //    m_NoRefrenceAssetMapList.Remove(item);
                //}
            }
        }
        return item;
    }

    /// <summary>
    /// 异步加载资源 （仅仅是不需要实例化得资源 比如音频 图片等）
    /// </summary>
    public void AsyncLoadResouce(string path, OnAsyncObjFinish dealFinish, ELoadResPriority priority, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = CRC32.GetCRC32(path);
        }
        ResourceItem item = GetCacheResouceItem(crc);
        if (item != null)
        {
            if (dealFinish != null)
            {
                dealFinish(path, item.m_Obj, param1, param2, param3);
            }
            return;
        }

        //判断是否在加载中
        AsyncLoadResParam param = null;
        if (!m_LoadingAssetDic.TryGetValue(crc, out param) || param == null)
        {
            param = m_AsyncLoadResParamPool.Spawn(true);
            param.m_Crc = crc;
            param.m_Path = path;
            param.m_Priority = priority;
            m_LoadingAssetDic.Add(crc, param);
            m_LoadingAssetList[(int)priority].Add(param);
        }

        //往回调列表里加回调
        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.m_DealFinish = dealFinish;
        callBack.m_Param1 = param1;
        callBack.m_Param2 = param2;
        callBack.m_Param3 = param3;
        param.m_CallBackList.Add(callBack);

    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        List<AsyncCallBack> callBackList = null;
        //上一次yield的时间
        long lastYieldTime = System.DateTime.Now.Ticks;

        while (true)
        {
            bool haveYield = false;
            for (int i = 0; i < (int)ELoadResPriority.RES_NUM; i++)
            {
                List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                if (loadingList.Count <= 0)
                {
                    continue;
                }
                AsyncLoadResParam loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callBackList = loadingItem.m_CallBackList;

                Object obj = null;
                ResourceItem item = null;
#if UNITY_EDITOR
                if (!m_LoadFromAssetBundle)
                {
                    obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
                    //模拟异步加载
                    yield return new WaitForSeconds(0.5f);
                    item = AssetBundleManager.Instance.FindResouceItem(loadingItem.m_Crc);
                }
#endif
                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadResouceAssetBundle(loadingItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest abRequest = null;
                        if (loadingItem.m_Sprite)
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }
                        else
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }

                        yield return abRequest;
                        if (abRequest.isDone)
                        {
                            obj = abRequest.asset;
                        }
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }

                CacheResouce(loadingItem.m_Path, ref item, loadingItem.m_Crc, obj, callBackList.Count);
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsyncCallBack callBack = callBackList[j];
                    if (callBack != null && callBack.m_DealFinish != null)
                    {
                        callBack.m_DealFinish(loadingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);
                        callBack.m_DealFinish = null;
                    }

                    callBack.Reset();
                    m_AsyncCallBackPool.Recycle(callBack);
                }

                obj = null;
                callBackList.Clear();
                m_LoadingAssetDic.Remove(loadingItem.m_Crc);

                loadingItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loadingItem);

                if (System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESTIME)
                {
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;//微秒
                    haveYield = true;
                }
            }
            if (!haveYield || System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESTIME)
            {
                lastYieldTime = System.DateTime.Now.Ticks;//微秒
                yield return null;
            }
        }
    }
}

//双向链表结构节点
public class DoubleLinkedListNode<T> where T : class, new()
{
    //前一个节点
    public DoubleLinkedListNode<T> prev = null;
    //后一个节点
    public DoubleLinkedListNode<T> next = null;

    public T t = null;
}

//双向链表结构
public class DoubleLinkedList<T> where T : class, new()
{
    //表头
    public DoubleLinkedListNode<T> Head = null;
    //表尾
    public DoubleLinkedListNode<T> Tail = null;
    //双向链表结构类对象池
    protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
    //个数
    protected int m_Count = 0;
    public int Count
    {
        get { return m_Count; }
    }

    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHeader(T t)
    {
        DoubleLinkedListNode<T> pNode = m_DoubleLinkNodePool.Spawn(true);
        pNode.next = null;
        pNode.prev = null;
        pNode.t = t;
        return AddToHeader(pNode);
    }
    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }
        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }
        m_Count++;
        return Head;
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToTail(pList);
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }
        pNode.next = null;
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }
        m_Count++;
        return Tail;
    }

    /// <summary>
    /// 移除某个节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return;
        }
        if (pNode == Head)
        {
            Head = pNode.next;
        }
        if (pNode == Tail)
        {
            Tail = pNode.next;
        }
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.next = pNode.prev = null;
        pNode.t = null;
        m_DoubleLinkNodePool.Recycle(pNode);
        m_Count--;
    }

    /// <summary>
    /// 把某个节点移动到头部
    /// </summary>
    /// <param name="pNode"></param>
    public void MoveToHead(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
        {
            return;
        }
        if (pNode.prev == null && pNode.next == null)
        {
            return;
        }
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }
        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

public class CMapList<T> where T : class, new()
{
    DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();
    Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

    //垃圾回收时自动调用
    ~CMapList()
    {
        Clear();
    }

    /// <summary>
    /// 清空列表
    /// </summary>
    public void Clear()
    {
        while (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }

    /// <summary>
    /// 插入一个基点到表头
    /// </summary>
    /// <param name="t"></param>
    public void InsertToHead(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DLink.AddToHeader(node);
            return;
        }
        m_DLink.AddToHeader(t);
        m_FindMap.Add(t, m_DLink.Head);
    }

    /// <summary>
    /// 从表尾弹出一个节点
    /// </summary>
    public void Pop()
    {
        if (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }

    /// <summary>
    /// 删除某个节点
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return;
        }
        m_DLink.RemoveNode(node);
        m_FindMap.Remove(t);
    }

    /// <summary>
    /// 获取到尾部节点
    /// </summary>
    /// <returns></returns>
    public T Back()
    {
        return m_DLink.Tail == null ? null : m_DLink.Tail.t;
    }

    /// <summary>
    /// 返回节点个数
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return m_FindMap.Count;
    }

    /// <summary>
    /// 查找是否存在该节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Find(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 刷新某个节点 把节点移动到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Reflesh(T t)
    {
        DoubleLinkedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return false;
        }
        m_DLink.MoveToHead(node);
        return true;
    }
}