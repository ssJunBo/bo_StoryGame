using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BundleEditor  {

    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
    //key 是ab包名  value 是路径  所有文件夹ab包dic
    public static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();

    //过滤list
    public static List<string> m_AllFileAB = new List<string>();

    //单个prefab的ab包
    public static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();

    [MenuItem("Tools/打包")]
    public static void Build()
    {
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName,fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
            }
        }
        
        //返回GUID 
        string[] allStr = AssetDatabase.FindAssets("t:Prefab",abConfig.m_AllPrefabPath.ToArray());
        for (int i = 0; i <allStr.Length ; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
         //   Debug.Log("prefabs文件夹下的所有prefab文件路径：" + path);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / allStr.Length);

            if (!ContainAllFileAB(path))
            {
                //找到pre
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                for (int j = 0; j < allDepend.Length; j++)
                {
                    Debug.Log(allDepend[j]);
                    if (!ContainAllFileAB(allDepend[j])&&!allDepend[j].EndsWith(".cs"))
                    {
                        m_AllFileAB.Add(allDepend[j]);
                        allDependPath.Add(allDepend[j]);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError("存在相同名字Prefab！名字 "+obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }

        foreach (string name in m_AllFileDir.Keys)
        {
            //siki任务15 5分钟
        }

        EditorUtility.ClearProgressBar();
    }

    static void SetABName(string name,string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter==null)
        {
            Debug.LogError("不存在此路径文件："+path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    static void SetABName(string name,List<string> paths) {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name,paths[i]);
        }
    }

    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path==m_AllFileAB[i]||path.Contains(m_AllFileAB[i]))
            {
                return true;
            }
        }
        return false;
    }
}
