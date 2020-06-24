using UnityEngine;

public class GameStart : MonoBehaviour
{
    private GameObject obj;
    void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
        obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Image.prefab",true);

    }

    void OnLoadFinish(string path, Object obj, object pa1, object pa2, object pa3)
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ObjectManager.Instance.ReleaseObject(obj);
            obj = null;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Image.prefab", true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ObjectManager.Instance.ReleaseObject(obj, 0, true);
            obj = null;
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
