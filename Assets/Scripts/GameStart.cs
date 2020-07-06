using UnityEngine;
using UnityEngine.EventSystems;

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
        //ObjectManager.Instance.PreLoadGameObject("Assets/GameData/Prefabs/Image.prefab", 20, false);

        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
            transform.Find("UIRoot/WndRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
            transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
        RegisterUI();

        UIManager.Instance.PopUpWnd("MenuPanel.prefab");
    }

    void RegisterUI() 
    {
        UIManager.Instance.Register<MenuUI>("MenuPanel.prefab");
    }

    
    void OnLoadFinish(string path, Object obj, object param1 = null, object param12 = null, object param13 = null)
    {
        objT = obj as GameObject;

    }

    private void Update()
    {

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
