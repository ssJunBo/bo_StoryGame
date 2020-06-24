using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    //对象池节点
    public Transform RecyclePoolTrs;
    //场景节点
    public Transform SceneTrs;
    //对象池
    protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
    //暂存ResObj的Dic
    protected Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
    //ResourceObj的类对象池
    protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool = null;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="recycleTrs">回收节点</param>
    /// <param name="sceneTrs">场景默认节点</param>
    public void Init(Transform recycleTrs, Transform sceneTrs)
    {
        m_ResourceObjClassPool = ObjectManager.Instance.GetOrCreateClassPool<ResourceObj>(1000);
        RecyclePoolTrs = recycleTrs;
        SceneTrs = sceneTrs;
    }

    /// <summary>
    /// 从对象池取对象
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    protected ResourceObj GetObjectFromPool(uint crc)
    {
        List<ResourceObj> st = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out st) && st != null && st.Count > 0)
        {
            ResourceManager.Instance.IncreaseResourceRef(crc);
            ResourceObj resObj = st[0];
            st.RemoveAt(0);
            GameObject obj = resObj.m_CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
                resObj.m_Already = false;
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }
            return resObj;
        }
        return null;
    }

    /// <summary>
    /// 同步加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bClear"></param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
    {
        uint crc = CRC32.GetCRC32(path);
        ResourceObj resourceObj = GetObjectFromPool(crc);
        if (resourceObj == null)
        {
            resourceObj = m_ResourceObjClassPool.Spawn(true);
            resourceObj.m_Crc = crc;
            resourceObj.m_bClear = bClear;
            //ResourceManager提供加载方法
            resourceObj = ResourceManager.Instance.LoadResource(path, resourceObj);

            if (resourceObj.m_ResItem.m_Obj != null)
            {
                resourceObj.m_CloneObj = GameObject.Instantiate(resourceObj.m_ResItem.m_Obj) as GameObject;
            }
        }

        if (setSceneObj)
        {
            resourceObj.m_CloneObj.transform.SetParent(SceneTrs, false);
        }

        int tempID = resourceObj.m_CloneObj.GetInstanceID();
        if (!m_ResourceObjDic.ContainsKey(tempID))
        {
            m_ResourceObjDic.Add(tempID, resourceObj);
        }

        return resourceObj.m_CloneObj;
    }

    /// <summary>
    /// 异步对象加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fealFinish"></param>
    /// <param name="priority"></param>
    /// <param name="setSceneObject"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="bClear"></param>
    public void InstantiateObjectAsync(string path,OnAsyncObjFinish dealFinish,ELoadResPriority priority,bool setSceneObject=false,object param1=null, object param2 = null, object param3 = null,bool bClear=true)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        uint crc = CRC32.GetCRC32(path);
        ResourceObj resObj = GetObjectFromPool(crc);
        if (resObj!=null)
        {
            if (setSceneObject)
            {
                resObj.m_CloneObj.transform.SetParent(SceneTrs,false);
            }
            if (dealFinish !=null)
            {
                dealFinish(path,resObj.m_CloneObj,param1, param2, param3);
            }

            return;
        }

        resObj = m_ResourceObjClassPool.Spawn(true);
        resObj.m_Crc = crc;
        resObj.m_SetSceneParent = setSceneObject;
        resObj.m_bClear = bClear;
        resObj.m_DealFinis = dealFinish;
        resObj.m_Param1 = param1;
        resObj.m_Param2 = param2;
        resObj.m_Param3 = param3;

        //调用ResourceManager的异步加载接口
        ResourceManager.Instance.AsyncLoadResource(path,resObj, OnLoadResourceObjFinish,priority);
    }

    void OnLoadResourceObjFinish(string path,ResourceObj resObj,object param1=null, object param2 = null, object param3 = null) { }

    /// <summary>
    /// 回收资源
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="maxCacheCount"></param>
    /// <param name="destroyCache"></param>
    /// <param name="recycleParent"></param>
    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destroyCache = false, bool recycleParent = true)
    {
        if (obj == null)
        {
            return;
        }
        ResourceObj resObj = null;
        int tempID = obj.GetInstanceID();
        if (!m_ResourceObjDic.TryGetValue(tempID,out resObj))
        {
            Debug.LogError(obj.name +" 对象不是ObjectManager创建的");
            return;
        }
        if (resObj==null)
        {
            Debug.LogError("缓存的ResourceObj为空！");
        }

        if (resObj.m_Already)
        {
            Debug.LogError("该对象已经放回对象池，检查自己是否清空引用！");
            return;
        }
#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        List<ResourceObj> st = null;
        if (maxCacheCount==0)
        {
            m_ResourceObjDic.Remove(tempID);
            ResourceManager.Instance.ReleaseResource(resObj,destroyCache);
            resObj.Reset();
            m_ResourceObjClassPool.Recycle(resObj);
        }
        else//回收到对象池
        {
            if (!m_ObjectPoolDic.TryGetValue(resObj.m_Crc,out st)||st==null)
            {
                st = new List<ResourceObj>();
                m_ObjectPoolDic.Add(resObj.m_Crc,st);
            }

            if (resObj.m_CloneObj)
            {
                if (recycleParent)
                {
                    resObj.m_CloneObj.transform.SetParent(RecyclePoolTrs);
                }
                else
                {
                    resObj.m_CloneObj.SetActive(false);
                }
            }

            if (maxCacheCount<=0||st.Count<maxCacheCount)
            {
                st.Add(resObj);
                resObj.m_Already = true;
                //ResourceManger做一个引用计数 
                ResourceManager.Instance.DecreaseResourceRef(resObj);
            }
            else
            {
                m_ResourceObjDic.Remove(tempID);
                ResourceManager.Instance.ReleaseResource(resObj,destroyCache);
                resObj.Reset();
                m_ResourceObjClassPool.Recycle(resObj); 
            }
        }
    }

    #region 类对象池使用
    protected Dictionary<Type, object> m_ClassPoolDic = new Dictionary<Type, object>();

    /// <summary>
    /// 创建类对象池，创建完成后外面可以保存ClassObjectPool<T>，然后调用spwan和recycle来创建和回收类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxcount"></param>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class, new()
    {
        Type type = typeof(T);
        object outObj = null;
        if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxcount);
            m_ClassPoolDic.Add(type, newPool);
            return newPool;
        }
        return outObj as ClassObjectPool<T>;
    }
    #endregion  
}
