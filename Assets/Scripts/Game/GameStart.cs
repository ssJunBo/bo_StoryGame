using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    public bool LoadFromAssetBundle;
    protected override void Awake()
    {
        base.Awake();
        GameObject.DontDestroyOnLoad(gameObject);
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

        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
            transform.Find("UIRoot/WndRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
            transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());

        //用到的窗口要进行注册
        RegisterUI();

        UIManager.Instance.PopUpWnd(ConStr.SPLASHPANEL);
        GameMapManager.Instance.Init(this);
        ToolsManager.TimeCallback(this, 1f, () => {
            GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
        });
    }

    //注册ui窗口
    void RegisterUI()
    {
        UIManager.Instance.Register<MenuUI>(ConStr.MENUPANEL);
        UIManager.Instance.Register<LoadingUI>(ConStr.LOADINGPANEL);
        UIManager.Instance.Register<SplahUI>(ConStr.SPLASHPANEL);
    }

    //加载配置表
    void LoadConfiger()
    {
        //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
        //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
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
