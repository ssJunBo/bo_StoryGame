using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : Window
{
    private MenuPanel m_MainPanel;

    public override void Awake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.m_StartButton, OnClickStart);
        AddButtonClickListener(m_MainPanel.m_LoadButton, OnClickLoad);
        AddButtonClickListener(m_MainPanel.m_ExitButton, OnClickExit);

        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test1.png", OnLoadSpriteTest1, ELoadResPriority.RES_SLOW, true);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test3.png", OnLoadSpriteTest3, ELoadResPriority.RES_HIGHT, true);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test2.png", OnLoadSpriteTest2, ELoadResPriority.RES_HIGHT, true);

    }

    void OnLoadSpriteTest1(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Test1.sprite = sp;
            Debug.Log("图片异步加载出来了！");
        }
    }
    void OnLoadSpriteTest2(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null) 
        {
            Sprite sp = obj as Sprite;
            Debug.Log("sp " + sp.GetType());
            m_MainPanel.m_Test2.sprite = sp;
            Debug.Log("图片2异步加载出来了！");
        }
    }
    void OnLoadSpriteTest3(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Test3.sprite = sp;
            Debug.Log("图片3异步加载出来了！");
        }
    }

    public override void OnUpdate()
    {

    }

    void OnClickStart()
    {
        Debug.Log("点击了开始游戏");
    }

    void OnClickLoad()
    {
        Debug.Log("点击了加载游戏");
    }

    void OnClickExit()
    {
        Debug.Log("点击了退出游戏");
    }
}
