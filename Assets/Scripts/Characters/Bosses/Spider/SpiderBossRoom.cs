using Cinemachine;
using DG.Tweening;
using System;
using UnityEngine;

using DreamSenker.CameraSystem;
using DreamSenker.Combat.Health;
using DreamSenker.Data.Runtime;
using DreamSenker.Managers;
using DreamSenker.Shared;

namespace DreamSenker.Characters.Bosses
{
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class SpiderBossRoom : MonoBehaviour
{
    [SerializeField] private SpiderController _spider;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private Transform _bossCameraCenterPoint;

    [SerializeField] private GameObject _leftWall;
    [SerializeField] private GameObject _rightWall;


    private Animator _animator;

    private int _animNameToHash_RoomWall = Animator.StringToHash("SpiderBossRoomWall");
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag) && !_spider.Health.IsDead && !GameManager.Instance.GameSaveData.IsMetSpiderBoss)
        {
            AudioManager.Instance.PlayMusic(GameResources.Instance.SpiderFightingClip);

            //玩家遇到Boss
            GameManager.Instance.GameSaveData.IsMetSpiderBoss = true;
            //关闭房间
            _animator.Play(_animNameToHash_RoomWall);

            _virtualCamera.Follow = null;
            Vector3 cameraNewPos = new Vector3(
                _bossCameraCenterPoint.position.x, _bossCameraCenterPoint.position.y, _virtualCamera.transform.position.z);
            _virtualCamera.transform.DOMove(cameraNewPos, 1f).
                SetEase(Ease.InOutQuad).
                OnComplete(() =>
                {
                    _spider.BindingPlayer(collision.transform);
                });
        }
    }
    private void OnEnable()
    {
        //关心Boss保持死亡事件
        EventCenter.Instance.AddEventListener<GameBossKeepDeadEventArgs>(E_EventType.Game_BossKeepDead, OnGameBossKeepDead);
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<GameBossKeepDeadEventArgs>(E_EventType.Game_BossKeepDead, OnGameBossKeepDead);
    }
    private void OnGameBossKeepDead(object eventSender, GameBossKeepDeadEventArgs args)
    {
        switch (args.BossType)
        {
            case EBossType.Spider:


                //_spider.KeepDeathState();
                Destroy(_spider.gameObject);
                _leftWall.SetActive(false);
                _rightWall.SetActive(false);
                break;
            case EBossType.FoxTwo:
                break;
            default:
                break;
        }
        
    }
}
}
