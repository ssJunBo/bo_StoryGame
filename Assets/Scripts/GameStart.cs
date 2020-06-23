using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    public AudioSource m_Audio;
    private AudioClip clip;
    void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
    }

    private void Start()
    {
        ResourceManager.Instance.PreloadRes("Assets/GameData/Sounds/menusound.mp3");

        // ResourceManager.Instance.AsyncLoadResouce("Assets/GameData/Sounds/menusound.mp3",OnLoadFinish,ELoadResPriority.RES_MIDDLE);

    }

    void OnLoadFinish(string path,Object obj,object pa1, object pa2, object pa3)
    {
        clip = obj as AudioClip;
        m_Audio.clip = obj as AudioClip;
        m_Audio.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //ResourceManager.Instance.ReleaseResource(clip,true);//true 表示资源不再次使用 彻底删除
            //m_Audio.clip = null;
            //clip = null;
            long time = System.DateTime.Now.Ticks;
            clip = ResourceManager.Instance.LoadResouce<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
            m_Audio.clip = clip;
            m_Audio.Play();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ResourceManager.Instance.ReleaseResource(clip,true);
            m_Audio.clip = null;
            clip = null;
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
