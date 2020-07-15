using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToolsManager 
{
    #region 定时回调系统
    static public Coroutine TimeCallback(MonoBehaviour mono,float time, UnityAction callBack)
    {
        return mono.StartCoroutine(Coroutine(time, callBack));
    }
    static IEnumerator Coroutine(float time, UnityAction callback)
    {
        yield return new WaitForSeconds(time);
        if (callback != null)
        {
            callback();
        }
    }
    #endregion

}
