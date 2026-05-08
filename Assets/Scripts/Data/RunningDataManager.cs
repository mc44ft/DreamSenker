using System.IO;
using UnityEngine;
/// <summary>
/// 运行时数据管理器
/// </summary>
public class RunningDataManager : BaseManager<RunningDataManager>
{
    private static string SAVE_DIR = Path.Combine(Application.persistentDataPath, "SaveData");
    private RunningDataManager() { }

    public void SaveData(IRunningData data)
    {
        JsonManager.Instance.SaveData(data.GetType().Name, data, JsonType.LitJson, SAVE_DIR);
    }
    public bool LoadData<T>(out T data) where T : class, IRunningData, new()
    {
        data = JsonManager.Instance.LoadData<T>(typeof(T).Name, JsonType.LitJson, SAVE_DIR);

        if (data != null)
        {
            return true;
        }
        else
        {
            Debug.Log($"无{typeof(T).Name}存档数据");
            return false;
        }
    }
    public bool DeleteData()
    {
        bool isSuccess = true;
        string searchPattern = "*.json";
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_DIR);
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
}
