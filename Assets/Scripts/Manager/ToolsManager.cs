using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class ToolsManager
{
    #region 定时回调系统
    static public Coroutine TimeCallback(MonoBehaviour mono, float time, UnityAction callBack, float time2 = -1, UnityAction callback2 = null)
    {
        return mono.StartCoroutine(Coroutine(time, callBack, time2, callback2));
    }
    static IEnumerator Coroutine(float time, UnityAction callback, float time2 = -1, UnityAction callback2 = null)
    {
        yield return new WaitForSeconds(time);
        if (callback != null)
        {
            callback();
        }
        if (time2 != -1)
        {
            yield return new WaitForSeconds(time2);
            if (callback2 != null)
            {
                callback2();
            }
        }
    }
    #endregion

    #region 隐藏与事件添加
    /// <summary>
    /// 设置物体显隐
    /// </summary>
    /// <param name="go"></param>
    /// <param name="bActive"></param>
    public static void SetActive(GameObject go, bool bActive)
    {
        if (go == null)
            return;
        if (go.activeSelf != bActive)
            go.SetActive(bActive);
    }
    /// <summary>
    /// 给GameObject添加监听
    /// </summary>
    /// <param name="go"></param>
    /// <param name="action"></param>
    public static void AddListenObj(GameObject go, UnityAction action)
    {
        Button btn = go.GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(action);
    }
    #endregion

    #region DoTween一些常用动画
    /// <summary>
    /// 渐隐渐现
    /// </summary>
    /// <param name="go"></param>
    /// <param name="fromvalue"></param>
    /// <param name="tovalue"></param>
    /// <param name="duration"></param>
    public static void PingpongTexe(GameObject go, float fromvalue, float tovalue, float duration)
    {
        Text te = go.GetComponent<Text>();
        if (te == null) return;
        te.DOFade(tovalue, duration).onComplete = () =>
        {
            PingpongTexe(go, tovalue, fromvalue, duration);
        };
    }

    #endregion
}
