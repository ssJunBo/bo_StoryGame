using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUI : BaseUI
{
    private ChapterPanel m_MainPanel;
    List<GameObject> itemLis = new List<GameObject>();
    public override void OnAwake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<ChapterPanel>();

        
        AddButtonClickListener(m_MainPanel.back_Btn, OnClickBack);
    }

    public override void OnStart(params object[] paraList)
    {
        itemLis.Clear();
        int count = (int)paraList[0];
        for (int i = 0; i < count; i++)
        {
            itemLis.Add(ObjectManager.Instance.SpwanObjFromPool(ConStr.bookItem_Path, targetTransform: m_MainPanel.content_Trs));
        }
    }

    void OnClickBack()
    {
        for (int i = 0; i < itemLis.Count; i++) 
        {
            ObjectManager.Instance.ReleaseObject(itemLis[i]);
        }
        GameManager.Instance.UIManager.HideWnd(ConStr._ChapterPanel);
    }

}
