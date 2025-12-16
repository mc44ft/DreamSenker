using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 运行时数据管理器
/// </summary>
public class RunningDataManager : BaseManager<RunningDataManager>
{
    private static string RUNNINGDATA_SAVEPATH = Path.Combine(Application.persistentDataPath, "SaveData");
    private RunningDataManager() { }

    public void SaveData(IRunningData data)
    {
        string json = JsonUtility.ToJson(data);

        //检查文件夹是否存在
        if (!Directory.Exists(RUNNINGDATA_SAVEPATH))
        {
            Directory.CreateDirectory(RUNNINGDATA_SAVEPATH);
        }

        string path = Path.Combine(RUNNINGDATA_SAVEPATH, data.GetType().Name + ".json");
        File.WriteAllText(path, json);

    }
    public bool LoadData<T>(out T data) where T : class, IRunningData, new()
    {
        string path = Path.Combine(RUNNINGDATA_SAVEPATH, typeof(T).Name + ".json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            data = new T();
            JsonUtility.FromJsonOverwrite(json, data);
            return true;    
        }
        else
        {
            Debug.Log($"无{typeof(T).Name}存档数据");

            data = null;
            return false;
        }
    }
    public bool DeleteData()
    {
        bool isSuccess = true;
        //通配符
        string searchPattern = "*.json";
        //获取文件夹信息
        DirectoryInfo directoryInfo = new DirectoryInfo(RUNNINGDATA_SAVEPATH);
        //使用通配符匹配所有json文件
        FileInfo[] fileInfos = directoryInfo.GetFiles(searchPattern);


        foreach(FileInfo file in fileInfos)
        {
            try
            {
                file.Delete();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"删除文件{file.Name} 失败:{e.Message}");
                isSuccess = false;
            }
        }
        return isSuccess;
    }
    /*[MenuItem("Tools/一键打开存档文件夹 %g")] 
    public static void OpenPersistentDataPath()
    {
        string path = Application.persistentDataPath;
        
        // 这一句就是那个“魔法传送”
        EditorUtility.RevealInFinder(path);
        
        Debug.Log("已打开路径: " + path);
    }*/
}
