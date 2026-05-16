using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace DreamSeeker.Characters.Player
{
    public class CameraFollowObject : MonoBehaviour
    {
        private Transform _target;
        private int _preRotationY;
        private Tween _rotateTween;
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SetUp(Transform player)
        {
            _target = player.Find("CameraFollowPoint");
            _target ??= player;

            transform.position = _target.position;
            transform.rotation = _target.rotation;
            
            _preRotationY = (int)_target.rotation.eulerAngles.y;
        }

        private void Update()
        {
            if (_target == null) return;
            transform.position = _target.position;
            int y = (int)_target.rotation.eulerAngles.y;
            if (y != _preRotationY)
            {
                Flip(y);
                _preRotationY = y;
            }
        }

        private void Flip(float targetY)
        {
            if (_rotateTween != null)
            {
                _rotateTween.Kill();
            }
            _rotateTween = transform.DORotate(new Vector3(0, targetY, 0), 0.5f);
        }

        private void OnDestroy()
        {
            _rotateTween?.Kill();
        }
    }
}