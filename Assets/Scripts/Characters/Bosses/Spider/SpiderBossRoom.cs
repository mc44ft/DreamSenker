using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Combat.Health;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Framework.Events;
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
        EventBus.Subscribe<GameBossKeepDeadEvent>(OnGameBossKeepDead);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<GameBossKeepDeadEvent>(OnGameBossKeepDead);
    }
    /// <summary>
    /// 根据事件中的 Boss 类型恢复对应的永久死亡场景状态。
    /// </summary>
    private void OnGameBossKeepDead(GameBossKeepDeadEvent eventData)
    {
        switch (eventData.BossType)
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
