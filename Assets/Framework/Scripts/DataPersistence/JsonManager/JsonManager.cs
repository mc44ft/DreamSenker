using LitJson;
using System.IO;
using UnityEngine;

public enum JsonType
{
    JsonUtility,
    LitJson
}

public class JsonManager
{
    private static JsonManager _instance = new JsonManager();
    public static JsonManager Instance = _instance;

    private JsonManager()
    {
    }
    /// <summary>
    /// JsonType.JsonUtility是Unity自带的Json转换器 缺陷很多 比如不支持字典 不支持非[Serializeble]的类 只处理public或[SerializeField]字段
    /// JsonType.LitJson是一个第三方的Json转换工具 弥补了JsonUtility的不足 但在极限性能下会不如JsonUtility（一般用这个即可）
    /// </summary>
    public void SaveData<T>(string fileName, T data, JsonType jsonType = JsonType.LitJson) where T : class, new()
    {
        string jsonStr = jsonType == JsonType.JsonUtility ? JsonUtility.ToJson(data) : JsonMapper.ToJson(data);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName + ".json"), jsonStr);
    }

    public T LoadData<T>(string fileName, JsonType jsonType = JsonType.LitJson) where T : class, new()
    {
        //先判断默认文件夹下是否有该文件 没有再读取初始文件夹中的文件
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            //读取默认文件夹下的数据
            path = Application.streamingAssetsPath + "/" + fileName + ".json";
        }
        if (!File.Exists(path))
        {
            return null;
        }
        string jsonStr = File.ReadAllText(path);
        return jsonType == JsonType.JsonUtility ? JsonUtility.FromJson<T>(jsonStr) : JsonMapper.ToObject<T>(jsonStr);
    }
    
}