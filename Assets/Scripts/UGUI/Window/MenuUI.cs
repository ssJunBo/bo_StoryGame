using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : Window
{
    private MenuPanel m_MainPanel;

    public override void Awake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.m_StartButton, OnClickStart);
        AddButtonClickListener(m_MainPanel.m_LoadButton, OnClickLoad);
        AddButtonClickListener(m_MainPanel.m_ExitButton, OnClickExit);
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
