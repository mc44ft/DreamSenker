using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamSenker.Data.Configs.Audio
{
[CreateAssetMenu(fileName = "AudioCollectionConfig_", menuName = "ScriptableObject/Config/Audio/AudioCollectionConfig")]
public class AudioCollectionSO : ScriptableObject
{
    public List<AudioData> AudioDatas;

    public AudioClip GetClip(string clipName)
    {
        return AudioDatas.Where(data => data.Name == clipName).FirstOrDefault().Clip;
    }

}
public class AudioData
{
    public string Name;
    public AudioClip Clip;
    [Range(0f, 1f)] 
    public float Volume;
}
}
