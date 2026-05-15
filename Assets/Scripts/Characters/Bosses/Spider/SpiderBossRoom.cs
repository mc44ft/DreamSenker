using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Combat.Health;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class SpiderBossRoom : MonoBehaviour
{
    [SerializeField] private SpiderController _spider;
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

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.EnterBossFight(_bossCameraCenterPoint.position, BindSpiderPlayer);
            }
            else
            {
                BindSpiderPlayer();
            }

            void BindSpiderPlayer()
            {
                _spider.BindingPlayer(collision.transform);
            }
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
