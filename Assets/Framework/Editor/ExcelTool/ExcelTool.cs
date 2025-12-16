using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#region Notice
//绝对路径：是指从盘符根目录开始的路径
//示例：D:/Exe/Unity/Project/Dungeon Gunner Course/Assets/SomeFolder
//相对路径：是从Asset开始的路径
//示例：Assets/SomeFolder

//像Directory.CreateDirectory()这样的方法 既接收绝对路径 也接收相对路径
//System.IO下的方法，FIle.WriteAllText、Directory.Exists 这样操作文件系统的方法 直接使用绝对路径是最稳妥的
//用UnityEditor.AssetDatabase下的方法去管理Unity资源时， 必须使用相对路径
#endregion
public class ExcelTool
{
    /// <summary>
    /// 数据行起始索引
    /// </summary>
    public static int BEGIN_INDEX = 4;

    /// <summary>
    /// 数据类存放的路径
    /// </summary>
    public static string DATA_CLASS_PATH = Path.Combine(Application.dataPath, "Scripts/Data/ExcelData/Data");

    /// <summary>
    /// Excel数据容器类存放的路径
    /// </summary>
    public static string DATA_CONTAINER_CLASS_PATH = Path.Combine(Application.dataPath, "Scripts/Data/ExcelData/Container");

    /// <summary>
    /// SO类存放的路径
    /// </summary>
    public static string SO_CLASS_PATH = Path.Combine(Application.dataPath, "Scripts/Data/ExcelData/SOClass");
    /// <summary>
    /// SO资源存放的路径
    /// 这里创建Unity资源 需要一个相对路径
    /// </summary>
    public static string SO_ASSET_PATH = Path.Combine("Assets/ScriptableObjectAssets/ExcelSO");

    /// <summary> 
    /// 读取Excel文件的路径
    /// </summary>
    public static string EXCEL_PATH = Path.Combine(Application.dataPath, "Framework/Editor/ArtRes/Excel");
    /// <summary>
    /// 读取对话系统Excel文件的路径
    /// </summary>
    public static string DIALOGUE_EXCEL_PATH = Path.Combine(Application.dataPath, "Framework/Editor/ArtRes/Excel/Dialogue");
    /// <summary>
    /// 普通Excel二进制处理
    /// </summary>
    [MenuItem("GameTool/ExcelTool/GenerateExcelInfo_Binary")]
    private static void GenerateExcelInfo_Binary()
    {
        GenerateExcelInfo(EXCEL_PATH, (table, excelName) =>
        {
            GenerateExcelDataClass(table, excelName);//生成Excel数据类
            GenerateExcelContainer(table, excelName);//生成Excel数据容器类
            GenerateExcelBinaryData(table, excelName);//生成二进制数据
        });
        AssetDatabase.Refresh();//刷新窗口
    }
    /// <summary>
    /// 对话系统Excel二进制处理（需手动创建数据类和数据容器类）
    /// </summary>
    [MenuItem("GameTool/ExcelTool/GenerateExcelInfo_Binary_Dialogue")]
    private static void GenerateExcelInfo_Binary_Dialogue()
    {
        GenerateExcelInfo(DIALOGUE_EXCEL_PATH, (table, excelName) =>
        {
            GenerateExcelBinaryData(table, excelName);//生成二进制数据
        });
        AssetDatabase.Refresh();//刷新窗口
    }
    /// <summary>
    /// SO类处理
    /// </summary>
    [MenuItem("GameTool/ExcelTool/GenerateExcelInfo_SO/GenerateExcelInfo_SO_Class")]
    private static void GenerateExcelInfo_SO_Class()
    {
        GenerateExcelInfo(EXCEL_PATH, (table, excelName) =>
        {
            GenerateExcelSOClass(table, excelName);
        });
        AssetDatabase.Refresh();//刷新窗口
    }
    /// <summary>
    /// SO资源处理
    /// </summary>
    [MenuItem("GameTool/ExcelTool/GenerateExcelInfo_SO/GenerateExcelInfo_SO_Asset")]
    private static void GenerateExcelInfo_SO_Asset()
    {
        GenerateExcelInfo(EXCEL_PATH, (table, excelName) =>
        {
            GenerateExcelInfoSOAsset(table, excelName);
        });

        //创建 Unity 的原生资源（.asset 文件）时调用 将修改的文件从内存保存到硬盘
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();//刷新窗口
    }
    /// <summary>
    /// 遍历所有Excel表
    /// </summary>
    /// <param name="excelPath"></param>
    /// <param name="callback"></param>
    private static void GenerateExcelInfo(string excelPath, Action<DataTable, string> callback)
    {
        //1.加载指定路径中的所有Excel文件
        //创建一个路径文件夹
        //如果存在该文件夹则返回已经存在的文件夹
        DirectoryInfo dInfo = Directory.CreateDirectory(excelPath);
        //获取所有的Excel文件
        //*。xlsx是一个通配符表达式 表示匹配所有以.xlsx结尾的文件 *表示匹配任意长度的字符
        FileInfo[] files = dInfo.GetFiles("*.xlsx");
        FileInfo[] moreFiles = dInfo.GetFiles("*.xls");
        //合并两个数组
        FileInfo[] allFiles = files.Concat(moreFiles).ToArray();

        DataTableCollection tableCollection;//存储Excel数据的DataSet对象

        for (int i = 0; i < allFiles.Length; i++)//遍历所有excel文件
        {
            using (FileStream fs = allFiles[i].Open(FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                tableCollection = excelReader.AsDataSet().Tables;
            }
            //获取无后缀的Excel文件名
            string excelName = Path.GetFileNameWithoutExtension(allFiles[i].Name);

            foreach (DataTable table in tableCollection)//遍历Excel文件中的每一张表
            {
                callback?.Invoke(table, excelName);
            }

        }
    }
    /// <summary>
    /// 将Excel表转换为二进制数据
    /// </summary>
    /// <param name="table"></param>

    private static void GenerateExcelBinaryData(DataTable table, string excelName)
    {
        string directoryPath = Path.Combine(BinaryDataMgr.EXCEL_BINARY_DATA_SAVE_PATH, excelName);
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
        //二进制数据文件的名称和数据类的类名一致
        string finalPath = Path.Combine(directoryPath, excelName + "_" + table.TableName + ".cao");
        using (FileStream fs = new FileStream(finalPath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            //存储总行数
            fs.Write(BitConverter.GetBytes(table.Rows.Count - BEGIN_INDEX), 0, sizeof(int));
            //存储总列数
            fs.Write(BitConverter.GetBytes(table.Columns.Count), 0, sizeof(int));

            //获取主键变量名字节数组
            string keyName = GetVariableNameRow(table)[GetKeyIndex(table)].ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(keyName);

            //存储主键变量名 字节数组的长度
            fs.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));

            //存储主键变量名的字节数组
            fs.Write(bytes, 0, bytes.Length);

            DataRow nameRow = GetVariableNameRow(table);
            DataRow typeRow = GetVariableTypeRow(table);//根据类型决定写入数据的方式
            DataRow row;
            for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
            {
                row = table.Rows[i];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    //存储字段名
                    string nameStr = nameRow[j].ToString();
                    byte[] nameBytes = Encoding.UTF8.GetBytes(nameStr);
                    //存储单个字段长度
                    fs.Write(BitConverter.GetBytes(nameBytes.Length), 0, sizeof(int));
                    //存储单个字段字符串字节数组
                    fs.Write(nameBytes, 0, nameBytes.Length);
                    switch (typeRow[j].ToString())
                    {
                        case "int":
                            fs.Write(BitConverter.GetBytes(int.Parse(row[j].ToString())), 0, sizeof(int));//存储每一行的int类型数据
                            break;
                        case "float":
                            fs.Write(BitConverter.GetBytes(float.Parse(row[j].ToString())), 0, sizeof(float));//存储每一行的float类型数据
                            break;
                        case "bool":
                            fs.Write(BitConverter.GetBytes(bool.Parse(row[j].ToString())), 0, sizeof(bool));//存储每一行的bool类型数据
                            break;
                        case "string":
                            bytes = Encoding.UTF8.GetBytes(row[j].ToString());
                            fs.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));//存储每个值的字节数组长度
                            fs.Write(bytes, 0, bytes.Length);//存储每个值的字节数组
                            break;
                        default:
                            Debug.LogError("未知类型：" + typeRow[j] + "\n表名" + table.TableName);
                            break;
                    }
                }
            }
        }

        AddCompletedInfo(table.TableName + "表的二进制数据生成完毕！存放于：" + directoryPath);
    }
    /// <summary>
    /// 生成数据类
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelDataClass(DataTable table, string excelName)
    {
        //字段名行
        DataRow nameRow = GetVariableNameRow(table);
        //字段类型行
        DataRow typeRow = GetVariableTypeRow(table);

        string directoryPath = Path.Combine(DATA_CLASS_PATH, excelName);
        //创建数据类存放的文件夹
        //如果存在返回该文件夹
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
        string className = excelName + "_" + table.TableName + "Data";
        string str = "public class " + className + "\n{\n";
        for (int i = 0; i < table.Columns.Count; i++)
        {
            str += "    public " + typeRow[i] + " " + nameRow[i] + ";\n";
        }
        str += "}";

        //保存文件
        string finalPath = Path.Combine(directoryPath, className + ".cs");
        File.WriteAllText(finalPath, str);

        AddCompletedInfo(table.TableName + "表的数据类生成完毕！存放于：" + directoryPath);
    }
    /// <summary>
    /// 生成数据容器类
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelContainer(DataTable table, string excelName)
    {
        int keyIndex = GetKeyIndex(table);
        DataRow rowType = GetVariableTypeRow(table);

        //检查文件夹
        string directoryName = Path.Combine(DATA_CONTAINER_CLASS_PATH, excelName);
        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);
        string className = excelName + "_" + table.TableName + "Container";
        string str = "using System.Collections.Generic;\n" +
                     "public class " + className + "\n" +
                     "{\n" +
                     "    public Dictionary<" + rowType[keyIndex] + ", " + excelName + "_" + table.TableName + "Data" + "> dataDict = new Dictionary<" + rowType[keyIndex] + ", " + excelName + "_" + table.TableName + "Data" + ">();\n" +
                     "}";

        string finalPath = Path.Combine(directoryName, className + ".cs");
        //保存文件
        File.WriteAllText(finalPath, str);

        AddCompletedInfo(table.TableName + "表的容器类生成完毕！存放于：" + directoryName);
    }
    
    /// <summary>
    /// 生成SO类
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelSOClass(DataTable table, string excelName)
    {
        //读取字段名行
        DataRow nameRow = GetVariableNameRow(table);
        //读取字段类型行
        DataRow typeRow = GetVariableTypeRow(table);

        string directoryPath = Path.Combine(SO_CLASS_PATH, excelName);
        //创建数据类存放的文件夹
        //如果存在返回该文件夹
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
        string className = excelName + "_" + table.TableName + "SO";
        string str = "using UnityEngine;" + "\n";
        str += "public class " + className + " : ScriptableObject" + "\n{\n";
        for (int i = 0; i < table.Columns.Count; i++)
        {
            str += "    public " + typeRow[i] + " " + nameRow[i] + ";\n";
        }
        str += "}";

        string finalPath = Path.Combine(directoryPath, className + ".cs");
        File.WriteAllText(finalPath, str);

        //创建cs脚本后，Unity会重新编译，Console的信息会被清空
        AddCompletedInfo(table.TableName + "表的SO类生成完毕！存放于：" + directoryPath);
    }
    /// <summary>
    /// 生成SO资源
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelInfoSOAsset(DataTable table, string excelName)
    {
        //存放SO资源文件的相对路径
        //生成SO资源文件只能使用相对路径
        string outputFolder = Path.Combine(SO_ASSET_PATH, excelName, table.TableName);

        //拼接出用于文件操作的绝对路径
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        string absoluteFolderPath = Path.Combine(projectRootPath, outputFolder);

        //使用绝对路径来操作文件系统
        if (!Directory.Exists(absoluteFolderPath))
            Directory.CreateDirectory(absoluteFolderPath);

        //拼接SO脚本的类名
        string soClassName = excelName + "_" + table.TableName + "SO";
        //默认程序集
        string assemblyQualifiedName = soClassName + ", Assembly-CSharp";
        Type soType = Type.GetType(assemblyQualifiedName);
        if (soType == null)
        {
            Debug.LogError("找不到名为" + soClassName + "的C#脚本");
            return;
        }

        DataRow nameRow = GetVariableNameRow(table);

        
        for(int i = BEGIN_INDEX; i < table.Rows.Count; i++)//数据行从第五行开始
        {
            DataRow dataRow = table.Rows[i];

            //创建一个SO实例
            ScriptableObject soInstance = ScriptableObject.CreateInstance(soType);
            for (int j = 0; j < table.Columns.Count; j++)
            {
                string fieldName = nameRow[j].ToString();
                string fieldValue = dataRow[j].ToString();

                //这个方法将字符串转换为指定类型
                FieldInfo fieldInfo = soType.GetField(fieldName);
                if(fieldInfo != null)
                {
                    object typedValue = Convert.ChangeType(fieldValue, fieldInfo.FieldType);
                    fieldInfo.SetValue(soInstance, typedValue);
                }
                    
            }
            //得到主键变量的值（转为字符串）
            string key = dataRow[GetKeyIndex(table)].ToString();
            string assetFieldName = soType.Name + "_" + key + ".asset";

            //这个方法可以将路径（前几个参数）和文件名（最后一个参数）正确拼接在一起
            //且在不同平台下都能正确拼接（不需要考虑Windows “/” 和mac “\” 的问题）
            //如果拼接的字符串第一个字符为“/”，将会把这个视为绝对路径，前面的所有路径都会被抛弃（这点要注意）
            //所以传入的参数前后都不需要带“/”
            string finalAssetPath = Path.Combine(outputFolder, assetFieldName);
            //这个方法只接受相对路径

            AssetDatabase.CreateAsset(soInstance, finalAssetPath);
        }

        //此处并非Unity脚本编译后执行
        Debug.Log(table.TableName + "表的SO资源全部生成完毕！存放于：" + absoluteFolderPath);
    }

    
    private static int GetKeyIndex(DataTable table)
    {
        if (table.Rows.Count > 2)
        {
            DataRow row = table.Rows[2];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (row[i].ToString() == "key" || row[i].ToString() == "Key")
                {
                    return i;
                }
            }
        }
        else
            Debug.LogError("配表不完整！");
        return 0;
    }

    /// <summary>
    /// 获取Excel表格中的变量名行
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private static DataRow GetVariableNameRow(DataTable table)
    {
        //获取第一行作为变量名行
        if (table.Rows.Count > 0)
        {
            return table.Rows[0];
        }
        else
        {
            Debug.LogError("配表不完整！");
            return null;
        }
    }

    /// <summary>
    /// 获取Excel表格中的变量类型行
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private static DataRow GetVariableTypeRow(DataTable table)
    {
        //获取第一行作为变量名行
        if (table.Rows.Count > 1)
        {
            return table.Rows[1];
        }
        else
        {
            Debug.LogError("配表不完整！");
            return null;
        }
    }
    /// <summary>
    /// Unity在cs脚本创建后会重新编译
    /// 编译会清空Console信息以及所有静态变量的值
    /// 使用此方法可以安全的在脚本编译完成后打印信息
    /// </summary>
    [DidReloadScripts]//此特性会让该函数在Unity脚本每次编译成功后自动调用
    private static void DebugCompletedInfo()
    {
        string message = SessionState.GetString("ExcelToolLog", "");
        if(!string.IsNullOrEmpty(message))
        {
            string[] messageArray = message.Split(',');
            //跳过开头的空字符串
            for (int i = 1; i < messageArray.Length; i++)
            {
                Debug.Log(messageArray[i]);
            }
            //Debug.Log(message);
            SessionState.EraseString("ExcelToolLog");
        }
    }
    private static void AddCompletedInfo(string info)
    {
        string oldMessage = SessionState.GetString("ExcelToolLog", "");
        string newMessage = oldMessage + ',' + info;
        SessionState.SetString("ExcelToolLog", newMessage);
    }
}