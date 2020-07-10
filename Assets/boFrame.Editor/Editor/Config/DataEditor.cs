using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.Collections.Generic;
using System.ComponentModel;
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
    public static void TestRead()
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
    public static void TestWriteExcel()
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

    [MenuItem("Tools/测试/测试已有类进行反射")]
    public static void TestReflection1()
    {
        TestInfo testInfo = new TestInfo()
        {
            Id = 2,
            Name = "测试反射",
            IsA = false,
            AllStrList = new List<string>(),
            AllTestInfoList = new List<TestInfoTwo>()

        };
        testInfo.AllStrList.Add("测试111111");
        testInfo.AllStrList.Add("测试222222");
        testInfo.AllStrList.Add("测试333333");

        for (int i = 0; i < 3; i++)
        {
            TestInfoTwo test = new TestInfoTwo();
            test.Id = i + 1;
            test.Name = i + "name";
            testInfo.AllTestInfoList.Add(test);
        }


        GetMemberValue(testInfo, "Name");

        //object list = GetMemberValue(testInfo, "AllStrList");
        //int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));
        //for (int i = 0; i < listCount; i++)
        //{
        //    object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });
        //    Debug.LogError(item);
        //}

        object list = GetMemberValue(testInfo, "AllTestInfoList");
        int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));

        for (int i = 0; i < listCount; i++)
        {
            object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });

            object id = GetMemberValue(item, "Id");
            object name = GetMemberValue(item, "Name");
            Debug.LogError(id + "    " + name);
        }


        //Debug.LogError(listCount);
    }

    [MenuItem("Tools/测试/测试已有数据进行反射")]
    public static void TestReflection2()
    {
        object obj = CreateClass("TestInfo");
        PropertyInfo info = obj.GetType().GetProperty("Id");
        SetValue(info, obj, "21", "int");
        PropertyInfo nameInfo = obj.GetType().GetProperty("Name");
        SetValue(nameInfo, obj, "sadjflka", "string");
        PropertyInfo isInfo = obj.GetType().GetProperty("IsA");
        SetValue(isInfo, obj, "true", "bool");
        PropertyInfo heighInfo = obj.GetType().GetProperty("Heigh");
        SetValue(heighInfo, obj, "10.5", "float");
        PropertyInfo enumInfo = obj.GetType().GetProperty("TestType");
        SetValue(enumInfo, obj, "VAR1", "enum");

        Type type = typeof(string);
        object list = CreateList(type);
        for (int i = 0; i < 3; i++)
        {
            object addItem = "测试填数据" + i;
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { addItem });//调用list的add方法 添加数据
        }
        obj.GetType().GetProperty("AllStrList").SetValue(obj, list, null);

        object twoList = CreateList(typeof(TestInfoTwo));
        for (int i = 0; i < 3; i++)
        {
            object addItem = CreateClass("TestInfoTwo");
            PropertyInfo itemIdInfo = addItem.GetType().GetProperty("Id");
            SetValue(itemIdInfo, addItem, "12" + i, "int");
            PropertyInfo nameIdInfo = addItem.GetType().GetProperty("Name");
            SetValue(nameIdInfo, addItem, "测试类啊" + i, "string");
            twoList.GetType().InvokeMember("Add", BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod, null, twoList, new object[] { addItem });
        }
        obj.GetType().GetProperty("AllTestInfoList").SetValue(obj, twoList, null);


        TestInfo testInfo = (obj as TestInfo);
        foreach (var str in testInfo.AllStrList)
        {
            Debug.LogError(str);
        }
        foreach (var test in testInfo.AllTestInfoList)
        {
            Debug.LogError(test.Id + "::" + test.Name);
        }

        Debug.LogError(testInfo.Id + "     " + testInfo.Name + "     "
            + testInfo.IsA + "     " + testInfo.Heigh + "     " + testInfo.TestType);
    }

    [MenuItem("Tools/Xml/Xml转excel")]
    public static void XmlToExcel()
    {
        string name = "MonsterData";
        string regPath = Application.dataPath + "/../Reg/MonsterData.xml";
        if (!File.Exists(regPath))
        {
            Debug.LogError("此数据不存在配置转换xml" + name);
        }

        XmlDocument xml = new XmlDocument();
        XmlReader reader = XmlReader.Create(regPath);
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;//忽略xml里面的注释
        xml.Load(reader);

        XmlNode xn = xml.SelectSingleNode("data");
        XmlElement xe = (XmlElement)xn;
        string className = xe.GetAttribute("name");
        string xmlName = xe.GetAttribute("to");
        string excelName = xe.GetAttribute("from");
        //存储所有变量的表
        Dictionary<string, SheetClass> allSheetClassDic = new Dictionary<string, SheetClass>();
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
        ReadXmlNode(xe, allSheetClassDic, 0);
        reader.Close();

        object data = null;
        Type type = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = asm.GetType(className);
            if (tempType != null)
            {
                type = tempType;
                break;
            }
        }
        if (type != null)
        {
            string xmlPath = XmlPath + className + ".xml";
            data = BinarySerializeOpt.XmlDeserialize(XmlPath, type);
        }

        List<SheetClass> outSheetList = new List<SheetClass>();
        foreach (SheetClass sheetClass in allSheetClassDic.Values)
        {
            if (sheetClass.Depth == 1)
            {
                outSheetList.Add(sheetClass);
            }
        }

        for (int i = 0; i < outSheetList.Count; i++)
        {
            ReadData(data, outSheetList[i], allSheetClassDic, sheetDataDic);
        }

    }

    private static void ReadData(object data, SheetClass sheetClass, Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic)
    {
        List<VarClass> varList = sheetClass.VarList;
        VarClass varClass = sheetClass.ParentVar;
        object dataList = GetMemberValue(data, varClass.Name);

        int listCount = Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default|BindingFlags.InvokeMethod,null,dataList,new object[] { }));

        SheetData sheetData = new SheetData();
        for (int i = 0; i < varList.Count; i++)
        {
            if (!string.IsNullOrEmpty(varList[i].Col))
            {
                sheetData.AllName.Add(varList[i].Col);
                sheetData.AllType.Add(varList[i].Type);
            }
        }

        for (int j = 0; j < varList.Count; j++)
        {
            if (varList[j].Type=="list")
            {

            }
        }
    }

    /// <summary>
    /// 递推读取配置
    /// </summary>
    /// <param name="xe"></param>
    private static void ReadXmlNode(XmlElement xmlElement, Dictionary<string, SheetClass> allSheetClassDic, int depth)
    {
        depth++;
        foreach (XmlNode node in xmlElement.ChildNodes)
        {
            XmlElement xe = (XmlElement)node;
            if (xe.GetAttribute("type") == "list")
            {
                XmlElement listEle = (XmlElement)node.FirstChild;

                VarClass parentVar = new VarClass()
                {
                    Name = xe.GetAttribute("name"),
                    Type = xe.GetAttribute("type"),
                    Col = xe.GetAttribute("col"),
                    DefaultValue = xe.GetAttribute("defaultValue"),
                    Foregin = xe.GetAttribute("foregin"),
                    SplitStr = xe.GetAttribute("split"),
                };

                if (parentVar.Type=="list")
                {
                    parentVar.ListName = ((XmlElement)xe.FirstChild).GetAttribute("Name");
                    parentVar.ListSheetName= ((XmlElement)xe.FirstChild).GetAttribute("sheetname");
                }

                SheetClass sheetClass = new SheetClass()
                {
                    Name = listEle.GetAttribute("name"),
                    SheetName = listEle.GetAttribute("sheetname"),
                    SplitStr = listEle.GetAttribute("split"),
                    MainKey = listEle.GetAttribute("mainKey"),
                    ParentVar = parentVar,
                    Depth = depth
                };

                if (!string.IsNullOrEmpty(sheetClass.SheetName))
                {
                    if (!allSheetClassDic.ContainsKey(sheetClass.SheetName))
                    {
                        //获取该类下所有的变量
                        foreach (XmlNode insideNode in listEle.ChildNodes)
                        {
                            XmlElement insideXe = (XmlElement)insideNode;
                            VarClass varClass = new VarClass()
                            {
                                Name = insideXe.GetAttribute("name"),
                                Type = insideXe.GetAttribute("type"),
                                Col = insideXe.GetAttribute("col"),
                                DefaultValue = insideXe.GetAttribute("defaultValue"),
                                Foregin = insideXe.GetAttribute("foregin"),
                                SplitStr = insideXe.GetAttribute("split"),
                            };

                            if (varClass.Type == "list")
                            {
                                varClass.ListName = ((XmlElement)insideXe.FirstChild).GetAttribute("Name");
                                varClass.ListSheetName = ((XmlElement)insideXe.FirstChild).GetAttribute("sheetname");
                            }

                            sheetClass.VarList.Add(varClass);
                        }
                        allSheetClassDic.Add(sheetClass.SheetName, sheetClass);
                    }
                }
                ReadXmlNode(listEle, allSheetClassDic, depth);
            }
        }
    }

    /// <summary>
    /// 反射new一个list
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static object CreateList(Type type)
    {
        Type listType = typeof(List<>);
        Type specType = listType.MakeGenericType(new Type[] { type });//确定list<>括号里填的类型
        return Activator.CreateInstance(specType, new object[] { });//new 出来这个list
    }

    /// <summary>
    /// 反射变量赋值
    /// </summary>
    /// <param name="info"></param>
    /// <param name="var"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    private static void SetValue(PropertyInfo info, object var, string value, string type)
    {
        object val = (object)value;
        if (type == "int")
        {
            val = Convert.ToInt32(val);
        }
        else if (type == "bool")
        {
            val = Convert.ToBoolean(val);
        }
        else if (type == "float")
        {
            val = Convert.ToSingle(val);
        }
        else if (type == "enum")
        {
            val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(val.ToString());
        }
        info.SetValue(var, val, null);
    }

    /// <summary>
    /// 反射类里面变量的具体数值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="memberName"></param>
    /// <param name="bindingFlags"></param>
    /// <returns></returns>
    private static object GetMemberValue(object obj, string memberName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
    {
        Type type = obj.GetType();
        MemberInfo[] members = type.GetMember(memberName, bindingFlags);
        switch (members[0].MemberType)
        {
            case MemberTypes.Field://获取变量值
                return type.GetField(memberName, bindingFlags).GetValue(obj);
            case MemberTypes.Property://获取属性值
                return type.GetProperty(memberName, bindingFlags).GetValue(obj, null);
            default:
                return null;
        }
    }

    /// <summary>
    /// 反射创建类的实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object CreateClass(string name)
    {
        object obj = null;
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
            obj = Activator.CreateInstance(type);
        }
        return obj;
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

public class SheetClass
{
    //所处父级var变量
    public VarClass ParentVar { get; set; }
    //深度
    public int Depth { get; set; }
    //类名
    public string Name { get; set; }
    //类对应的sheet名
    public string SheetName { get; set; }
    //主键
    public string MainKey { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    //所包含的变量
    public List<VarClass> VarList = new List<VarClass>();
}

public class VarClass
{
    //原类里面变量的名称
    public string Name { get; set; }
    //变量类型
    public string Type { get; set; }
    //变量对应的excel里的列
    public string Col { get; set; }
    //变量的默认值
    public string DefaultValue { get; set; }
    //变量是list的话 外联部分列
    public string Foregin { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    //如果自己是list 对应的list类名
    public string ListName { get; set; }
    //如果自己是list 对应的sheet名
    public string ListSheetName { get; set; }

}

public class SheetData
{
    public List<string> AllName = new List<string>();
    public List<string> AllType = new List<string>();
    public List<RowData> AllData = new List<RowData>();
}

public class RowData
{
    public Dictionary<string, string> RowDataDic = new Dictionary<string, string>();
}

public enum TestEnum
{
    None = 0,
    VAR1 = 1,
    TEST2 = 2
}

public class TestInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsA { get; set; }
    public float Heigh { get; set; }
    public TestEnum TestType { get; set; }

    public List<string> AllStrList { get; set; }
    public List<TestInfoTwo> AllTestInfoList { get; set; }
}

public class TestInfoTwo
{
    public int Id { get; set; }
    public string Name { get; set; }
}
