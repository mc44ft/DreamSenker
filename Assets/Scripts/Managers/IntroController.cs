using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System;

namespace DreamSeeker.Managers
{
    /// <summary>
    /// 用于播放视频的组件
    /// </summary>
    public class IntroController : SingletonMono<IntroController>
    {
        [SerializeField] private GameObject RawImageGameObject;
        private VideoPlayer _videoPlayer;

        private Action _onfinished;
        protected override void Awake()
        {
            base.Awake();
            _videoPlayer = GetComponent<VideoPlayer>();
            
        }
        private void Start()
        {
            _videoPlayer.loopPointReached += (vp) =>
            {
                PlayEnd();
            };
        }

        void Update()
        {
            if (Input.anyKeyDown)
            {
                PlayEnd();
            }
        }
        private void PlayEnd()
        {
            RawImageGameObject.SetActive(false);

            _videoPlayer.Stop();
            _onfinished?.Invoke();
            _onfinished = null;
        }
        public void PlayVideo(VideoClip clip, Action onFinished)
        {
            RawImageGameObject.SetActive(true);

            _videoPlayer.clip = clip;

            _videoPlayer.Play();
            _onfinished = onFinished;
            
        }
    }
}
