using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    public bool LoadFromAssetBundle;

    #region UI挂点
    public Image bgImg;
    public GameObject beginTe;
    #endregion

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
    private SceneManager gameMapManager;
    public SceneManager MapManager
    {
        get { return gameMapManager; }
    }
    #endregion

    private void InitManager()
    {
        uiManager = new UIManager();
        gameMapManager = new SceneManager(this);
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
               transform.Find("UICamera").GetComponent<Camera>(),
               transform.Find("EventSystem").GetComponent<EventSystem>());

        //用到的窗口要进行注册
        RegisterUI();
        //预加载几个提示框
        ObjectManager.Instance.PreLoadGameObject(ConStr.tipsItem_Path, 2);
        //点击开始渐隐渐显效果
        ToolsManager.PingpongTexe(beginTe.gameObject, 1, 0, 1);
        ToolsManager.AddListenObj(bgImg.gameObject, GoMenuScene);

    }

    void GoMenuScene()
    {
        MapManager.LoadScene(ConStr.MENUSCENE);
        ToolsManager.SetActive(bgImg.gameObject, false);
    }

    private void Update()
    {
        UIManager.OnUpdate();
        if (Input.GetKeyDown(KeyCode.D))
        {
            ShowTips("你好啊全世界 ！" + Random.Range(0, 5).ToString());
        }
    }

    //注册ui窗口
    void RegisterUI()
    {
        UIManager.Register<MenuUI>(ConStr._MenuPanel);
        UIManager.Register<LoadingUI>(ConStr._LoadingPanel);
        UIManager.Register<ChapterUI>(ConStr._ChapterPanel);
    }

    /// <summary>
    /// 加载配置表 需要什么配置表都在这里加载
    /// </summary>
    void LoadConfiger()
    {
        //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
        //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
    }

    /// <summary>
    /// 提示展示
    /// </summary>
    /// <param name="strContent"></param>
    public void ShowTips(string strContent)
    {
        GameObject tipObj = ObjectManager.Instance.SpwanObjFromPool(ConStr.tipsItem_Path, targetTransform: UIManager.m_WndRoot);
        tipObj.GetComponent<TipsItem>().content.text = strContent;
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
