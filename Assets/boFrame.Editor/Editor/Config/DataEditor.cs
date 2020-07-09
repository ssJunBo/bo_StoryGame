using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using OfficeOpenXml;
//Messagepack 水果堂用的插件 类似于这种Epplus

public class DataEditor
{
    public static string XmlPath = "Assets/GameData/Data/Xml/";
    public static string BinaryPath = "Assets/GameData/Data/Binary/";
    public static string ScriptsPath = "Assets/Scripts/Data/";

    [MenuItem("Assets/类转xml")]
    public static void AssetsClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的类转成xml",
                "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            ClassToXml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Xml转Binary")]
    public static void AssetsXmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的Xml转成二进制",
                "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            XmlToBinary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Xml/所有Xml转成二进制")]
    public static void AllXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + XmlPath;
        string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < filesPath.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找文件夹下的xml",
               "正在扫描" + filesPath[i] + "... ...", 1.0f / filesPath.Length * i);
            if (filesPath[i].EndsWith(".xml"))
            {
                string tempPath = filesPath[i].Substring(filesPath[i].LastIndexOf("/") + 1);
                tempPath = tempPath.Replace(".xml", "");
                XmlToBinary(tempPath);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/测试/测试读取xml")]
    public static void TextRead()
    {
        string xmlPath = Application.dataPath + "/../Data/Reg/MonsterData.xml";
        XmlReader reader = null;
        try
        {
            XmlDocument xml = new XmlDocument();
            reader = XmlReader.Create(xmlPath);
            xml.Load(reader);
            XmlNode xn = xml.SelectSingleNode("data");
            XmlElement xe = (XmlElement)xn;
            string className = xe.GetAttribute("name");
            string xmlName = xe.GetAttribute("to");
            string excelName = xe.GetAttribute("from");
            reader.Close();
            Debug.LogError(className + " " + excelName + " " + xmlName);

            foreach (XmlNode node in xe.ChildNodes)
            {
                XmlElement tempXe = (XmlElement)node;
                string name = tempXe.GetAttribute("name");
                string type = tempXe.GetAttribute("type");
                Debug.LogError(name + " " + type);
                XmlNode listNode = tempXe.FirstChild;
                XmlElement listElement = (XmlElement)listNode;
                string listName = listElement.GetAttribute("name");
                string sheetName = listElement.GetAttribute("sheetname");
                string mainKey = listElement.GetAttribute("mainKey");
                foreach (XmlNode nd in listElement.ChildNodes)
                {
                    XmlElement txe = (XmlElement)nd;
                    Debug.LogError(txe.GetAttribute("name") + " - "
                        + txe.GetAttribute("col") + " - "
                        + txe.GetAttribute("type"));
                }
                Debug.LogError(listName + " - " + sheetName + " - " + mainKey);
            }
        }
        catch (Exception e)
        {
            if (reader != null)
            {
                reader.Close();
            }
            Debug.LogError(e);
            throw;
        }
    }

    [MenuItem("Tools/测试/测试写入excel")]
    public static void TextWriteExcel()
    {
        string xlsxPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
        FileInfo xlsxFile = new FileInfo(xlsxPath);
        if (xlsxFile.Exists)
        {
            xlsxFile.Delete();
            xlsxFile = new FileInfo(xlsxPath);
        }
        using (ExcelPackage package = new ExcelPackage(xlsxFile))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("怪物配置");
            //worksheet.DefaultColWidth = 10;//sheet页面默认行宽度
            //worksheet.DefaultRowHeight = 30;//sheet页面默认列高度
            //worksheet.Cells.Style.WrapText = true;//设置所有单元格的自动换行
            //worksheet.InsertColumn()//插入行 从某一行开始插入多少行
            //worksheet.InsertRow() //插入列 从某一列开始插入多少列
            //worksheet.DeleteColumn() //删除行 从某一行开始删除多少行
            //worksheet.DeleteRow() //删除列 从某一列开始删除多少列
            //worksheet.Column(1).Width
            //worksheet.Column(1).Hidden

            ExcelRange range = worksheet.Cells[1, 1];
            range.Value = "测试2222222222\n2222222222222222";
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.None;
            //range.Style.Fill.BackgroundColor.SetColor
            //range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;//对齐方式
            range.AutoFitColumns();//自适应宽度
            range.Style.WrapText = true;//自动换行
            package.Save();
        }
    }

    /// <summary>
    /// xml转二进制
    /// </summary>
    /// <param name="name"></param>
    private static void XmlToBinary(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }
            if (type != null)
            {
                string xmlPath = XmlPath + name + ".xml";
                string binaryPath = BinaryPath + name + ".bytes";
                object obj = BinarySerializeOpt.XmlDeserialize(xmlPath, type);
                BinarySerializeOpt.BinarySerialize(binaryPath, obj);
            }

        }
        catch (Exception e)
        {
            Debug.LogError(name + "xml转二进制失败！ 异常信息：" + e);
        }
    }

    /// <summary>
    /// 实际的类转xml
    /// </summary>
    /// <param name="name"></param>
    private static void ClassToXml(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            Type type = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }
            if (type != null)
            {
                var temp = Activator.CreateInstance(type);//相当与new我们的这个类
                if (temp is ExcelBase)
                {
                    (temp as ExcelBase).Construction();
                }
                string xmlPath = XmlPath + name + ".xml";
                BinarySerializeOpt.Xmlserialize(xmlPath, temp);
                Debug.Log(name + "类转xml成功，xml路径为: " + xmlPath);
            }
        }
        catch (Exception)
        {
            Debug.LogError(name + "类转xml失败！");
        }

    }
}
