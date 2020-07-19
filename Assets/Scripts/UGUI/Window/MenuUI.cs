using UnityEngine;
using UnityEngine.UI;

public class MenuUI : BaseUI
{
    private MenuPanel m_MainPanel;
    public override void OnAwake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<MenuPanel>();

        AddButtonClickListener(m_MainPanel.pianzhang_btn, OnClickChapter);
        AddButtonClickListener(m_MainPanel.chengjiu_btn , OnClickAchievement);
        AddButtonClickListener(m_MainPanel.newgame_btn , OnClickNewGame);
        AddButtonClickListener(m_MainPanel.goon_btn, OnClickGoOn);

        m_MainPanel.head_img.sprite = ResourceManager.Instance.LoadResouce<Sprite>("Assets/GameData/UGUI/test/test2.png");


        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test/test1.png", OnLoadSpriteTest1, ELoadResPriority.RES_SLOW, true);
        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test/test3.png", OnLoadSpriteTest3, ELoadResPriority.RES_HIGHT, true);
        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test/test2.png", OnLoadSpriteTest2, ELoadResPriority.RES_HIGHT, true);
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
        }
    }
    void OnLoadSpriteTest2(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
        }
    }
    void OnLoadSpriteTest3(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
        }
    }

    public override void OnUpdate()
    {

    }

    void OnClickChapter()
    {
        GameManager.Instance.UIManager.PopUpWnd(ConStr._ChapterPanel,paraList:5);
    }

    void OnClickAchievement()
    {
        GameManager.Instance.ShowTips("奥哦i");
    }

    void OnClickNewGame()
    {
        Debug.Log("点击了新的游戏");
    }

    void OnClickGoOn()
    {
        Debug.Log("点击了继续游戏");
    }
}
