using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoFrameConfig : ScriptableObject
{
    //打包时生成AB包配置表的二进制路径
    public string m_ABBytePath;
    //打包的默认名称
    public string m_AppName;
    //xml文件夹路径
    public string m_XmlPath;
    //二进制文件夹路径
    public string m_BinaryPath;
    //脚本文件夹路径
    public string m_ScriptsPath;
}

public class BoConfig
{
    private const string BoFramePath = "Assets/boFrame.Editor/Editor/BoFrameConfig.asset";
    public static BoFrameConfig GetBoFrame()
    {
        return AssetDatabase.LoadAssetAtPath<BoFrameConfig>(BoFramePath);
    }
}
