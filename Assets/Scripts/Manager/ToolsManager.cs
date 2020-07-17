using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

}
