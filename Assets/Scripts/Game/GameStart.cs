using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    public bool LoadFromAssetBundle;

    private GameObject objT;
    protected override void Awake()
    {
        base.Awake();

        ResourceManager.Instance.m_LoadFromAssetBundle = LoadFromAssetBundle;
        GameObject.DontDestroyOnLoad(gameObject);
        
        //从ab包加载就要先加载配置表
        if (ResourceManager.Instance.m_LoadFromAssetBundle)
        {
            AssetBundleManager.Instance.LoadAssetBundleConfig();
        }

        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
            transform.Find("UIRoot/WndRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
            transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());

        RegisterUI();

        GameMapManager.Instance.Init(this);

        ObjectManager.Instance.PreLoadGameObject(ConStr.ATTACK, 5);


        //ResourceManager.Instance.PreloadRes(ConStr.MENUSOUND);
        //AudioClip clip= ResourceManager.Instance.LoadResouce<AudioClip>(ConStr.MENUSOUND);
        //ResourceManager.Instance.ReleaseResource(clip);

        //GameObject obj = ObjectManager.Instance.InstantiateObject(ConStr.ATTACK, true, false);
        //ObjectManager.Instance.ReleaseObject(obj);
        //obj = null;

        //ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/ABABABA.prefab");

        GameMapManager.Instance.Init(this);
        GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
    }

    void RegisterUI()
    {
        UIManager.Instance.Register<MenuUI>(ConStr.MENUPANEL);
        UIManager.Instance.Register<LoadingUI>(ConStr.LOADINGPANEL);
    }

    private void Update()
    {
        UIManager.Instance.OnUpdate();
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
