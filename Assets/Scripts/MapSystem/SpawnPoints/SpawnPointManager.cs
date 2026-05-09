using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using DreamSenker.Shared;

namespace DreamSenker.MapSystem.SpawnPoints
{
public class SpawnPointManager : BaseManager<SpawnPointManager>
{
    /// <summary>
    /// 切换地图时的出生点
    /// </summary>
    private Dictionary<string, SpawnPoint> m_spawnPointDict_changeMap = new Dictionary<string, SpawnPoint>();
    /// <summary>
    /// 存档点
    /// </summary>
    private Dictionary<string, SpawnPoint> m_spawnPointDict_savePoint = new Dictionary<string, SpawnPoint>();
    /// <summary>
    /// 传送点
    /// </summary>
    private Dictionary<string, SpawnPoint> m_spawnPointDict_teleportPoint = new Dictionary<string, SpawnPoint>();
    private SpawnPointManager() { }

    public void RegisterSpawnPoint(SpawnPoint spawnPoint)
    {
        //拿到用于区分的前缀
        string prefix = new string(spawnPoint.ID.TakeWhile(c => c != '_').ToArray());
        if(prefix == "ChangeMap")
        {
            if (!m_spawnPointDict_changeMap.ContainsKey(spawnPoint.ID))
                m_spawnPointDict_changeMap.Add(spawnPoint.ID, spawnPoint);
            else
                Debug.LogWarning("该存档点已存在" + spawnPoint.ID);
        }
        else if(prefix == "Save")
        {
            if (!m_spawnPointDict_savePoint.ContainsKey(spawnPoint.ID))
                m_spawnPointDict_savePoint.Add(spawnPoint.ID, spawnPoint);
            else
                Debug.LogWarning("该存档点已存在" + spawnPoint.ID);
        }
        else if(prefix == "Teleport")
        {
            if (!m_spawnPointDict_teleportPoint.ContainsKey(spawnPoint.ID))
                m_spawnPointDict_teleportPoint.Add(spawnPoint.ID, spawnPoint);
            else
                Debug.LogWarning("该存档点已存在" + spawnPoint.ID);
        }
        else
        {
            Debug.Log("出生点ID填写错误！" + spawnPoint.ID);
        }
    }
    public void UnregisterSavePoint(SpawnPoint spawnPoint)
    {
        if (m_spawnPointDict_changeMap.ContainsKey(spawnPoint.ID))
            m_spawnPointDict_changeMap.Remove(spawnPoint.ID);
        if (m_spawnPointDict_savePoint.ContainsKey(spawnPoint.ID))
            m_spawnPointDict_savePoint.Remove(spawnPoint.ID);
        if (m_spawnPointDict_teleportPoint.ContainsKey(spawnPoint.ID))
            m_spawnPointDict_teleportPoint.Remove(spawnPoint.ID);
    }
    /// <summary>
    /// 切换场景时清空当前场景的所有出生点
    /// </summary>
    public void ClearSpawnPointDict()
    {
        m_spawnPointDict_changeMap.Clear();
        m_spawnPointDict_savePoint.Clear();
        m_spawnPointDict_teleportPoint.Clear();
    }
    public Vector3 GetSpawnPositionFromID(string spawnPointID)
    {
        if (m_spawnPointDict_savePoint.ContainsKey(spawnPointID))
            return m_spawnPointDict_savePoint[spawnPointID].transform.position;
        if (m_spawnPointDict_changeMap.ContainsKey(spawnPointID))
            return m_spawnPointDict_changeMap[spawnPointID].transform.position;
        if (m_spawnPointDict_teleportPoint.ContainsKey(spawnPointID))
            return m_spawnPointDict_teleportPoint[spawnPointID].transform.position;
        
        return Vector3.zero;
    }
    public Vector3 GetSpawnPositionFromSpawnType(ESpawnType eSpawnType)
    {
        SpawnPoint spawnPoint = m_spawnPointDict_changeMap.Where(pair => pair.Value.SpawnType == eSpawnType).FirstOrDefault().Value;

        if(spawnPoint != null)
        {
            return spawnPoint.transform.position;
        }
        return Vector3.zero;
    }
}
}
