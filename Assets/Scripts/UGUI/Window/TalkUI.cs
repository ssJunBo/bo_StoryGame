using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkUI : BaseUI
{
    private TalkPanel m_MainPanel;

    int num = 0;

    public override void OnAwake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<TalkPanel>();
       
    }

    public override void OnStart(params object[] paraList)
    {
        m_MainPanel.talkScrollView.Init(NormalCallBack);
        num = (int)paraList[0];
        m_MainPanel.talkScrollView.ShowList((int)paraList[0]);
        AddButtonClickListener(m_MainPanel.back_btn, OnClickBackBtn);
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            num++;
            m_MainPanel. talkScrollView.ShowList(num);
            m_MainPanel.talkScrollView.GoBottom();
        }
    }

    void NormalCallBack(GameObject cell, int index)
    {
        TalkItem talkItem = cell.GetComponent<TalkItem>();
        if (index % 2 != 0)
        {
            ToolsManager.SetActive(talkItem.headLeft.gameObject, true);
            ToolsManager.SetActive(talkItem.textLeft.gameObject, true);
            talkItem.textLeft.text = index.ToString() + "世界终究在我脚下";
        }
        else
        {
            ToolsManager.SetActive(talkItem.headRight.gameObject, true);
            ToolsManager.SetActive(talkItem.textRight.gameObject, true);
            talkItem.textRight.text = index.ToString() + "世界终究在我脚下";
        }
    }

    void OnClickBackBtn()
    {
        GameManager.Instance.UIManager.HideWnd(ConStr._TalkPanel);
        GameManager.Instance.UIManager.PopUpWnd(ConStr._MenuPanel);
    }
}
