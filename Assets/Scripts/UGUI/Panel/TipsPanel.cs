using UnityEngine;

public class TipsPanel : MonoBehaviour
{
    private void Start()
    {
        ToolsManager.TimeCallback(this,10f,()=> 
        {
           // ObjectManager.Instance.ReleaseObject(this.gameObject, 5);
        });
    }
}
