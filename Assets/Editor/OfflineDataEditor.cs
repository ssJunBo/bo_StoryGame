using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OfflineDataEditor  {

    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据","正在修改："+objects[i]+".......",1.0f/objects.Length*i);
            CreateOfflineData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CreateOfflineData(GameObject obj)
    {
        OfflineData offLineData = obj.GetComponent<OfflineData>();
        if (offLineData==null)
        {
            offLineData = obj.AddComponent<OfflineData>();
        }

        offLineData.BindData();

        EditorUtility.SetDirty(obj);
        Debug.Log("修改了"+obj.name+" prefab ! ");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}
