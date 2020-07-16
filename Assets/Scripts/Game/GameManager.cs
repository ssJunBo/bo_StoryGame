using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoSingleton<GameManager>
{
    public bool LoadFromAssetBundle;

    #region Manager 属性
    private UIManager uiManager;
    public UIManager UIManager
    {
        get { return uiManager; }
    }
    private AudioManager audioManager;
    public AudioManager AudioManager
    {
        get { return audioManager; }
    }
    private MapManager gameMapManager;
    public MapManager MapManager
    {
        get { return gameMapManager; }
    }
    #endregion

    private void InitManager()
    {
        uiManager = new UIManager();
        gameMapManager = new MapManager();
        audioManager = new AudioManager();
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitManager();

        //从ab包加载就要先加载配置表
        ResourceManager.Instance.m_LoadFromAssetBundle = LoadFromAssetBundle;
        if (ResourceManager.Instance.m_LoadFromAssetBundle)
            AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
        LoadConfiger();

        UIManager.Init(transform.Find("UIRoot") as RectTransform,
               transform.Find("UIRoot/WndRoot") as RectTransform,
               transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
               transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());

        //用到的窗口要进行注册
        RegisterUI();

        UIManager.PopUpWnd(ConStr.SPLASHPANEL);
        MapManager.Init(this);
        ToolsManager.TimeCallback(this, 1f, () =>
        {
            UIManager.CloseWnd(ConStr.SPLASHPANEL);
            MapManager.LoadScene(ConStr.MENUSCENE);
        });

        ObjectManager.Instance.PreLoadGameObject(ConStr.Attack_Path, 2);
        ObjectManager.Instance.PreLoadGameObject(ConStr.TipsPanel_Path,5);

        GameObject tipObj = ObjectManager.Instance.InstantiateObject(ConStr.TipsPanel_Path);
        tipObj.transform.SetParent(UIManager.m_WndRoot);
        tipObj.GetComponent<UIOfflineData>().ResetProp();

    }

    //注册ui窗口
    void RegisterUI()
    {
        UIManager.Register<MenuUI>(ConStr.MENUPANEL);
        UIManager.Register<LoadingUI>(ConStr.LOADINGPANEL);
        UIManager.Register<SplashUI>(ConStr.SPLASHPANEL);
    }

    //加载配置表
    void LoadConfiger()
    {
        //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
        //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
    }

    private void Update()
    {
        UIManager.OnUpdate();
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
