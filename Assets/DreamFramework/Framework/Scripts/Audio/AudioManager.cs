using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : SingletonAutoMono<AudioManager>
{

    /// <summary>
    /// 背景音乐播放组件
    /// </summary>
    private AudioSource _musicAudioSource;
    private AudioSource _soundAudioSource;

    //--------------- ALL VOLUME CONTROLLER --------------------------
    public float MusicVolume { get; private set; } = 0.6f;
    public float SoundVolume { get; private set; } = 0.8f;

    //private bool _soundIsPause = false;

    private void Awake()
    {
        //初始化MusicAudioSource
        _musicAudioSource = gameObject.AddComponent<AudioSource>();
        _musicAudioSource.volume = MusicVolume;
        _musicAudioSource.loop = true;
        _musicAudioSource.playOnAwake = false;
        _musicAudioSource.spatialBlend = 0f;//此项为零表示2D效果

        //初始化SoundAudioSourceOneShot
        _soundAudioSource = gameObject.AddComponent<AudioSource>();
        _soundAudioSource.volume = SoundVolume;
        _soundAudioSource.loop = true;
        _soundAudioSource.playOnAwake = false;
        _soundAudioSource.spatialBlend = 0f;

    }

    #region 音乐

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="musicName"></param>
    public void PlayMusic(AudioClip musicclip)
    {
        //动态创建播放背景音乐的组件 过场景不移除 保证背景音乐过场景时也能播放
        if (_musicAudioSource == null)
            return;
        _musicAudioSource.clip = musicclip;
        _musicAudioSource.Play();
    }

    public void StopMusic()
    {
        if (_musicAudioSource == null)
            return;
        _musicAudioSource.Stop();
    }

    public void PauseMusic()
    {
        if (_musicAudioSource == null)
            return;
        _musicAudioSource.Pause();
    }
    public void ResumeMusic()
    {
        if (_musicAudioSource == null)
            return;
        _musicAudioSource.UnPause();
    }
    public void ChangeMusicVolume(float volume)
    {
        MusicVolume = volume;
        if (_musicAudioSource == null)
            return;
        _musicAudioSource.volume = MusicVolume;
    }

    #endregion 音乐

    #region OneShot音效

    //默认是2D音效
    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySound(AudioClip soundClip, bool isOneShot = true)
    {
        if (_soundAudioSource == null)
            return;
        if (!isOneShot)
        {
            _soundAudioSource.clip = soundClip;
            _soundAudioSource.Play();
        }
        else
        {
            //这个方法可以同时播放多次音效 不会打断其他音效或音乐
            _soundAudioSource.PlayOneShot(soundClip);
        }
            
    }
    
    /// <summary>
    /// 停止音效播放
    /// </summary>
    /// <param name="audioSource">循环音效的组件</param>
    public void StopSound(AudioSource audioSource = null)
    {
        if(audioSource != null)
        {
            audioSource.Stop();
        }
        else
        {
            _soundAudioSource.Stop();
        }
    }
    public void PauseSoundAll()
    {
        if (_soundAudioSource == null)
            return;
        _soundAudioSource.Pause();
    }
    public void ResumeSoundAll()
    {
        if(_soundAudioSource == null)
            return;
        _soundAudioSource.UnPause();
    }
    /// <summary>
    /// 更改音效音量
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeSoundVolume(float volume)
    {
        SoundVolume = volume;
        if (_soundAudioSource == null)
            return;
        //更改正在播放的音效的音量
        _soundAudioSource.volume = volume;
    }
    #endregion 音效
}