using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EUIMshID
{
    None = 0,
}

public class UIManager
{
    //UI节点
    private RectTransform m_UiRoot;
    //窗口节点
    public RectTransform m_WndRoot;
    //UI摄像机
    private Camera m_UICamera;
    //EventSystem 节点
    private EventSystem m_EventSystem;
    //屏幕的宽高比
    private float m_CanvasRate = 0;

    private string m_UIPrefabPath = "Assets/GameData/Prefabs/UGUI/Panel/";
    /// <summary>
    /// 注册的字典
    /// </summary>
    private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, System.Type>();
    /// <summary>
    /// 所有打开的窗口
    /// </summary>
    private Dictionary<string, BaseUI> m_WindowDic = new Dictionary<string, BaseUI>();
    /// <summary>
    /// 打开的窗口列表
    /// </summary>
    private List<BaseUI> m_WindowList = new List<BaseUI>();


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="uiRoot">UI父节点</param>
    /// <param name="wndRoot">窗口父节点</param>
    /// <param name="uiCamera">UI摄像机</param>
    public void Init(RectTransform uiRoot, RectTransform wndRoot, Camera uiCamera, EventSystem eventSystem)
    {
        m_UiRoot = uiRoot;
        m_WndRoot = wndRoot;
        m_UICamera = uiCamera;
        m_EventSystem = eventSystem;
        m_CanvasRate = Screen.height / (m_UICamera.orthographicSize * 2);
    }

    /// <summary>
    /// 设置所有节目UI路径
    /// </summary>
    /// <param name="path"></param>
    public void SetUIPrefabPath(string path)
    {
        m_UIPrefabPath = path;
    }

    /// <summary>
    /// 显示或者隐藏所有UI
    /// </summary>
    public void ShowOrHideUI(bool show)
    {
        if (m_UiRoot != null)
        {
            m_UiRoot.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// 设置默认选择对象
    /// </summary>
    public void SetNormalSelectObj(GameObject obj)
    {
        if (m_EventSystem == null)
        {
            m_EventSystem = EventSystem.current;
        }
        m_EventSystem.firstSelectedGameObject = obj;
    }

    /// <summary>
    /// 窗口的更新
    /// </summary>
    public void OnUpdate()
    {
        for (int i = 0; i < m_WindowList.Count; i++)
        {
            if (m_WindowList[i] != null)
            {
                m_WindowList[i].OnUpdate();
            }
        }
    }

    /// <summary>
    /// 窗口注册方法
    /// </summary>
    /// <typeparam name="T">窗口泛型类</typeparam>
    /// <param name="name">窗口名</param>
    public void Register<T>(string name) where T : BaseUI
    {
        m_RegisterDic[name] = typeof(T);
    }

    /// <summary>
    /// 发送消息给窗口
    /// </summary>
    /// <param name="name">窗口名</param>
    /// <param name="msgID">消息ID</param>
    /// <param name="paraList">参数数组</param>
    /// <returns></returns>
    public bool SendMessageToWnd(string name, EUIMshID msgID = 0, params object[] paraList)
    {
        BaseUI wnd = FindWndByName<BaseUI>(name);
        if (wnd != null)
        {
            return wnd.OnMessage(msgID, paraList);
        }
        return false;
    }

    /// <summary>
    /// 根据窗口名查找窗口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T FindWndByName<T>(string name) where T : BaseUI
    {
        BaseUI wnd = null;
        if (m_WindowDic.TryGetValue(name, out wnd))
        {
            return (T)wnd;
        }
        return null;
    }

    /// <summary>
    /// 打开窗口 如果不存在则加载 存在直接show出来
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="bTop"></param>
    /// <param name="para1"></param>
    /// <param name="para2"></param>
    /// <param name="para3"></param>
    /// <returns></returns>
    public BaseUI PopUpWnd(string wndName, bool bTop = true, params object[] paraList)
    {
        BaseUI wnd = FindWndByName<BaseUI>(wndName);
        if (wnd == null || wnd.GameObject.activeSelf)
        {
            System.Type tp = null;
            if (m_RegisterDic.TryGetValue(wndName, out tp))
            {
                //继承自mono的无法用此方法实例化出来
                wnd = System.Activator.CreateInstance(tp) as BaseUI;
            }
            else
            {
                Debug.LogError("找不到窗口对应的脚本，窗口名是：" + wndName);
                return null;
            }

            GameObject wndObj = ObjectManager.Instance.SpwanObjFromPool(m_UIPrefabPath + wndName, false, false);
            if (wndObj == null)
            {
                Debug.Log("创建创建口Prefab失败：" + wndName);
                return null;
            }
            if (!m_WindowDic.ContainsKey(wndName))
            {
                m_WindowDic.Add(wndName, wnd);
                m_WindowList.Add(wnd);
            }
            //可以不用写 减少GC
            //wnd.name = wndName;
            wnd.GameObject = wndObj;
            wnd.Transform = wndObj.transform;
            wnd.Name = wndName;
            wnd.OnAwake(paraList);
            wndObj.transform.SetParent(m_WndRoot, false);
            if (bTop)
            {
                wndObj.transform.SetAsLastSibling();
            }
            wnd.OnStart(paraList);

            if (wnd.GameObject != null && !wnd.GameObject.activeSelf)
            {
                wnd.GameObject.SetActive(true);
            }
            if (bTop) wnd.Transform.SetAsLastSibling();
        }
        else
        {
            ShowWnd(wndName, bTop, paraList);
        }
        return wnd;
    }

    /// <summary>
    /// 根据窗口名关闭窗口
    /// </summary>
    /// <param name="name"></param>
    /// <param name="destroy"></param>
    public void CloseWnd(string name, bool destroy = false)
    {
        BaseUI wnd = FindWndByName<BaseUI>(name);
        CloseWnd(wnd, destroy);
    }

    /// <summary>
    /// 根据窗口对象关闭窗口 从字典中移除
    /// </summary>
    /// <param name="window"></param>
    /// <param name="destroy"></param>
    public void CloseWnd(BaseUI window, bool destroy = false)
    {
        if (window != null)
        {
            window.OnDisable();
            window.OnClose();
            if (m_WindowDic.ContainsKey(window.Name))
            {
                m_WindowDic.Remove(window.Name);
                m_WindowList.Remove(window);
            }

            if (destroy)
            {

                ObjectManager.Instance.ReleaseObject(window.GameObject, 0, true);
            }
            else
            {
                ObjectManager.Instance.ReleaseObject(window.GameObject, recycleParent: false);
            }
            window.GameObject = null;
            window = null;
        }
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAllWnd()
    {
        for (int i = m_WindowList.Count - 1; i >= 0; i--)
        {
            CloseWnd(m_WindowList[i]);
        }
    }

    /// <summary>
    /// 切换到唯一窗口
    /// </summary>
    public void SwitchStateByName(string name, bool bTop = true, params object[] paraList)
    {
        CloseAllWnd();
        PopUpWnd(name, bTop, paraList);
    }

    /// <summary>
    /// 根据名字隐藏窗口
    /// </summary>
    /// <param name="name"></param>
    public void HideWnd(string name)
    {
        BaseUI wnd = FindWndByName<BaseUI>(name);
        HideWnd(wnd);
    }

    /// <summary>
    /// 根据窗口对象隐藏窗口
    /// </summary>
    /// <param name="wnd"></param>
    public void HideWnd(BaseUI wnd)
    {
        if (wnd != null)
        {
            wnd.GameObject.SetActive(false);
            wnd.OnDisable();
        }
    }

    /// <summary>
    /// 根据窗口名字显示窗口
    /// </summary>
    /// <param name="name"></param>
    public void ShowWnd(string name, bool bTop = true, params object[] paraList)
    {
        BaseUI wnd = FindWndByName<BaseUI>(name);
        ShowWnd(wnd, bTop, paraList);
    }

    /// <summary>
    /// 根据窗口对象显示窗口
    /// </summary>
    /// <param name="wnd"></param>
    /// <param name="paraList"></param>
    public void ShowWnd(BaseUI wnd, bool bTop = true, params object[] paraList)
    {
        if (wnd != null)
        {
            if (wnd.GameObject != null && !wnd.GameObject.activeSelf)
            {
                wnd.GameObject.SetActive(true);
            }
            if (bTop) wnd.Transform.SetAsLastSibling();

            //if (!ObjectManager.Instance.IsObjectManagerCreate(wnd.GameObject))
            //{
            //    Debug.Log("不是从对象池创建的 ？？？ ");
            //}
            wnd.OnStart(paraList);
        }

    }

}
