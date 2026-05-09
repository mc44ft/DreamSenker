using UnityEngine;

using DreamSenker.Data.Runtime;

namespace DreamSenker.Data.Configs
{
[CreateAssetMenu(fileName = "GameConfig_", menuName = "ScriptableObject/Config/GameConfig")]
public class GameConfigSO : ScriptableObject
{
    [field: SerializeField] public GameSaveData SaveData { get; private set; }



#if UNITY_EDITOR
    //private E_MapSceneName _previousMapSceneName;
    //private void OnValidate()
    //{
    //    if(SaveData.SaveSceneName != _previousMapSceneName)
    //    {
    //        switch (SaveData.SaveSceneName)
    //        {
    //            case E_MapSceneName.CampMap:
    //                SaveData.SaveMapName = "CampMap";
    //                break;
    //            case E_MapSceneName.MagicMap:
    //                SaveData.SaveMapName = "MagicMap";
    //                break;
    //            case E_MapSceneName.CaveMap:
    //                SaveData.SaveMapName = "CaveMap";
    //                break;
    //            case E_MapSceneName.FoxMap:
    //                SaveData.SaveMapName = "FoxMap";
    //                break;
    //            case E_MapSceneName.MirrorMap1:
    //                SaveData.SaveMapName = "MirrorMap1";
    //                break;
    //            case E_MapSceneName.MirrorMap2:
    //                SaveData.SaveMapName = "MirrorMap2";
    //                break;
    //            default:
    //                break;
    //        }
    //        _previousMapSceneName = SaveData.SaveSceneName;
    //    }
    //}
#endif
}
}
