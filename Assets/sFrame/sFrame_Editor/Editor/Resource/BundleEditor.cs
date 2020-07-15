using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class BundleEditor
{
    private static string m_BundleTargetPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();

    private static string m_BundleTargetLoaclPath = Application.streamingAssetsPath;
    private static string ABCONFIGPATH = "Assets/sFrame/sFrame_Editor/Editor/Resource/ABConfig.asset";
    private static string ABBYTEPATH = BoConfig.GetBoFrame().m_ABBytePath;
    //"Assets/GameData/Data/ABData/AssetBundleConfig.bytes";

    //key 是ab包名  value 是路径  所有文件夹ab包dic
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    //过滤list
    private static List<string> m_AllFileAB = new List<string>();
    //单个prefab的ab包
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    //储存所有有效路径
    private static List<string> m_ConfigFil = new List<string>();

    //给外部调用 打包使用
    public static void Build()
    {
        DataEditor.AllXmlToBinary();
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        m_ConfigFil.Clear();
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
                m_ConfigFil.Add(fileDir.Path);
            }
        }

        //返回GUID 
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            //Debug.Log("prefabs文件夹下的所有prefab文件路径：" + path);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / allStr.Length);
            m_ConfigFil.Add(path);

            //检查过滤列表是否包含该路径
            if (!ContainAllFileAB(path))
            {
                //找到pre
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                for (int j = 0; j < allDepend.Length; j++)
                {
                    //Debug.Log(allDepend[j]);
                    if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
                    {
                        m_AllFileAB.Add(allDepend[j]);
                        allDependPath.Add(allDepend[j]);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError("存在相同名字Prefab！名字 " + obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }

        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }

        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }

        #region 加上下列代码,耗时会非常长长长！！！！
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
        #endregion

        BuildAssetBundle(m_BundleTargetPath);

        string[] oldABName = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABName.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABName[i], true);
            EditorUtility.DisplayProgressBar("清除AB包名", "名字" + oldABName[i], i / 1.0f / oldABName.Length);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/AB包/打ab包到StreamingAssets下")]//编辑器下使用
    public static void BuildAB()
    {
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        m_ConfigFil.Clear();
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
                m_ConfigFil.Add(fileDir.Path);
            }
        }

        //返回GUID 
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            //Debug.Log("prefabs文件夹下的所有prefab文件路径：" + path);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / allStr.Length);
            m_ConfigFil.Add(path);

            //检查过滤列表是否包含该路径
            if (!ContainAllFileAB(path))
            {
                //找到pre
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                for (int j = 0; j < allDepend.Length; j++)
                {
                    //Debug.Log(allDepend[j]);
                    if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
                    {
                        m_AllFileAB.Add(allDepend[j]);
                        allDependPath.Add(allDepend[j]);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError("存在相同名字Prefab！名字 " + obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }

        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }

        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }

        #region 加上下列代码,耗时会非常长长长！！！！
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
        #endregion

        BuildAssetBundle(m_BundleTargetLoaclPath);

        string[] oldABName = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABName.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABName[i], true);
            EditorUtility.DisplayProgressBar("清除AB包名", "名字" + oldABName[i], i / 1.0f / oldABName.Length);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/AB包/删除StreamingAssets下ab包")]//编辑器下使用
    public static void DeleteLocalAB()
    {
        string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo direction = new DirectoryInfo(m_BundleTargetLoaclPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (File.Exists(files[i].FullName))
            {
                File.Delete(files[i].FullName);
            }
        }
        AssetDatabase.Refresh();
    }

    static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("不存在此路径文件：" + path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }

    static void BuildAssetBundle(string path)
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        //key 为全路径 value 为包名
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                if (allBundlePath[j].EndsWith(".cs") || !ValidPath(allBundlePath[j]))
                    continue;

                Debug.Log("此ab包" + allBundles[i] + " 下面包含的资源文件路径：" + allBundlePath[j]);
                resPathDic.Add(allBundlePath[j], allBundles[i]);

            }
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        DeleteAB(path);
        //生成自己的配置表
        WriteData(resPathDic);

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression,
             EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            Debug.LogError("AssetBundle 打包失败！");
        }
        else
        {
            Debug.Log("AssetBundle 打包完毕！");

        }
    }

    static void WriteData(Dictionary<string, string> resPathDic)
    {
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<ABBase>();
        foreach (string path in resPathDic.Keys)
        {
            if (ValidPath(path)) continue;

            ABBase abBase = new ABBase();
            abBase.Path = path;
            abBase.Crc = CRC32.GetCRC32(path);
            abBase.ABName = resPathDic[path];
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
            abBase.ABDependce = new List<string>();
            string[] resDependce = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < resDependce.Length; i++)
            {
                string tempPath = resDependce[i];
                if (tempPath == path || path.EndsWith(".cs"))
                    continue;

                string abName = null;
                if (resPathDic.TryGetValue(tempPath, out abName))
                {
                    if (abName == resPathDic[path])
                        continue;

                    if (!abBase.ABDependce.Contains(abName))
                    {
                        abBase.ABDependce.Add(abName);
                    }

                }
            }
            config.ABList.Add(abBase);
        }

        //写入XML
        string xmlPath = Application.dataPath + "/AssetbundleConfig.xml";
        if (File.Exists(xmlPath))
            File.Delete(xmlPath);
        FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(config.GetType());
        xs.Serialize(sw, config);
        sw.Close();
        fileStream.Close();

        //写入二进制
        foreach (ABBase abBase in config.ABList)
        {
            abBase.Path = "";
        }

        FileStream fs = new FileStream(ABBYTEPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, config);
        fs.Close();
        AssetDatabase.Refresh();

        SetABName("assetbundleconfig", ABBYTEPATH);
    }

    /// <summary>
    /// 删除无用的AB包
    /// </summary>
    static void DeleteAB(string tarPath)
    {
        string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo direction = new DirectoryInfo(tarPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (ContainABName(files[i].Name, allBundlesName) || files[i].Name.EndsWith(".meta")
                || files[i].Name.EndsWith(".manifest") || files[i].Name.EndsWith("assetbundleconfig"))
            {
                continue;
            }
            else
            {
                Debug.Log("此AB包已经被删除或改名：" + files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
                if (File.Exists(files[i].FullName + ".manifest"))
                {
                    File.Delete(files[i].FullName + ".manifest");
                }
            }
        }
    }

    /// <summary>
    /// 遍历文件夹里的文件名与设置的所有AB包进行检查判断
    /// </summary>
    /// <param name="name"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    static bool ContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name == strs[i])
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否包含在已经有的AB包里 用来做ab冗余剔除
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path == m_AllFileAB[i] || path.Contains(m_AllFileAB[i]) && (path.Replace(m_AllFileAB[i], "")[0] == '/'))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否有效路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigFil.Count; i++)
        {
            if (path.Contains(m_ConfigFil[i]))
            {
                return true;
            }
        }
        return false;
    }
}
