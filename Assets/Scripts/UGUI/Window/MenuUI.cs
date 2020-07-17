﻿using UnityEngine;
using UnityEngine.UI;

public class MenuUI : BaseUI
{
    private MenuPanel m_MainPanel;
    public override void OnAwake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<MenuPanel>();

        AddButtonClickListener(m_MainPanel.m_StartButton, OnClickStart);
        AddButtonClickListener(m_MainPanel.m_LoadButton, OnClickLoad);
        AddButtonClickListener(m_MainPanel.m_ExitButton, OnClickExit);

        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test1.png", OnLoadSpriteTest1, ELoadResPriority.RES_SLOW, true);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test3.png", OnLoadSpriteTest3, ELoadResPriority.RES_HIGHT, true);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test2.png", OnLoadSpriteTest2, ELoadResPriority.RES_HIGHT, true);
    }

    void LoadMonsterData()
    {
        MonsterData monsterData = ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        for (int i = 0; i < monsterData.AllMonster.Count; i++)
        {
            Debug.Log(string.Format("ID:{0} 名字:{1} 外观:{2} 高度:{3} 稀有度:{4}", monsterData.AllMonster[i].Id, monsterData.AllMonster[i].Name, monsterData.AllMonster[i].OutLook, monsterData.AllMonster[i].Height, monsterData.AllMonster[i].Rare));
        }
    }

    void OnLoadSpriteTest1(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Test1.sprite = sp;
        }
    }
    void OnLoadSpriteTest2(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Test2.sprite = sp;
        }
    }
    void OnLoadSpriteTest3(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Test3.sprite = sp;
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
        Debug.Log("什 么 情 况 呢  ?");
        GameManager.Instance.UIManager.PopUpWnd(ConStr._TipsPanel,true);
    }

    void OnClickExit()
    {
        Debug.Log("点击了退出游戏");
    }
}
