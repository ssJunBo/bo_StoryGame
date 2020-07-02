using UnityEngine;

public class GameStart : MonoBehaviour
{
    private GameObject objT;
    void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
         //ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Image.prefab", OnLoadFinish, ELoadResPriority.RES_HIGHT,true);

        ObjectManager.Instance.PreLoadGameObject("Assets/GameData/Prefabs/Image.prefab", 20, false);
    }

    void OnLoadFinish(string path, Object obj, object param1=null, object param12 = null, object param13 = null)
    {
        objT = obj as GameObject;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ObjectManager.Instance.ReleaseObject(objT);
            objT = null;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            //obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Image.prefab", true);
            ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Image.prefab", OnLoadFinish, ELoadResPriority.RES_HIGHT, true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ObjectManager.Instance.ReleaseObject(objT, 0, true);
            objT = null;
        }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
        Debug.Log(" 清 空 编 辑 器 缓 存 ！");
#endif
    }
}
