using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : BaseUI
{

    private LoadingPanel m_MainPanel;
    private string m_SceneName;

    public override void OnAwake(params object[] paraList)
    {
        m_MainPanel = GameObject.GetComponent<LoadingPanel>();
        m_SceneName = (string)paraList[0];
    }

    public override void OnUpdate()
    {
        if (m_MainPanel == null) return;

        m_MainPanel = GameObject.GetComponent<LoadingPanel>();
        m_MainPanel.m_Slider.value = SceneManager.LoadingProgress / 100.0f;
        m_MainPanel.m_Text.text = string.Format("{0}%", SceneManager.LoadingProgress);
        if (SceneManager.LoadingProgress >= 99)
        {
            LoadOtherScene();
        }
    }

    /// <summary>
    /// 加载对应场景第一个ui
    /// </summary>
    public void LoadOtherScene()
    {
        //根据场景名字打开对应场景第一个界面
        switch (m_SceneName)
        {
            case ConStr.MENUSCENE:
                GameManager.Instance.UIManager.PopUpWnd(ConStr._MenuPanel);
                break;
            default:
                break;
        }
        GameManager.Instance.UIManager.CloseWnd(ConStr._LoadingPanel);
    }

}
