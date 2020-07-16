using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipsUI : BaseUI
{
    private TipsPanel m_MainPanel;
    private string m_SceneName;

    public override void OnAwake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<TipsPanel>();
        m_SceneName = (string)paraList[0];
    }

    public override void OnUpdate()
    {
        if (m_MainPanel == null) return;
    }

}
