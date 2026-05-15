using System.Linq;
using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
public class FoxBossRoom : MonoBehaviour, IFoxBossRoom
{
    [Header("BOSS")]
    [SerializeField] private FoxBoss _foxBoss;


    [SerializeField] private BoxCollider2D _battleZone;
    [SerializeField] private BoxCollider2D _groundBoxCollider;
    [SerializeField] private Animator _leftWallAnimator;

    [Header("CAMERA DETAILS")]
    [SerializeField] private Transform _cameraCenterPoint;
    [Space(5)]
    [Header("SKILL POINTS")]
    [SerializeField] private Transform[] _showPointArray;
    [Header("CLONE POINTS")]
    [SerializeField] private Transform[] _clonePointArray;
    [SerializeField] private Transform _cloneCenterPoint;
    //固定参数
    private Vector2 _foxBossBoundsSize;//Boss碰撞盒子大小
    private float _leftWallX;//Boss房左侧墙壁世界坐标X值
    private float _rightWallX;//Boss房右侧墙壁世界坐标X值

    //用于调节墙壁内间距的值
    private float _wallPaddingValue = 0.1f;

    private Transform _player;//Boss房间记录闯入的玩家

    private void Start()
    {
        //计算固定参数
        _foxBossBoundsSize = _foxBoss.GetComponent<Collider2D>().bounds.size;
        _leftWallX = transform.position.x - _battleZone.bounds.size.x / 2 + _wallPaddingValue;
        _rightWallX = transform.position.x + _battleZone.bounds.size.x / 2 - _wallPaddingValue;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            GameManager.Instance.GameSaveData.IsMetFoxBoss = true;

            AudioManager.Instance.PlayMusic(GameResources.Instance.FoxFightingClip);

            _leftWallAnimator.Play("FoxBossRoomLeftWall");
            _player = collision.transform;

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.EnterBossFight(_cameraCenterPoint.position, InitializeBoss);
            }
            else
            {
                InitializeBoss();
            }

            void InitializeBoss()
            {
                //相机到位后再绑定Boss目标
                _foxBoss.Initialize(collision.transform, this);
            }
        }
    }
    ///// <summary>
    ///// 返回Boss瞬移的位置，并保证其不超出区域边界
    ///// </summary>
    ///// <returns></returns>
    //public Vector3 GetRandomShowPosition(float radius, Vector3 playerPosition, float playerWidth)
    //{
    //    //Random.Value在0和1之间随机取值
    //    int rightMultiplier = Random.value < 0.5f ? -1 : 1;

    //    Vector3 newPos = new Vector3(playerPosition.x + rightMultiplier * radius, playerPosition.y, playerPosition.z);

    //    if(newPos.x < _leftWallX)//超出左边界
    //    {
    //        newPos.x = _leftWallX + playerWidth / 2;
    //    }
    //    else if(newPos.x > _rightWallX)//超出右边界
    //    {
    //        newPos.x = _rightWallX - playerWidth / 2;
    //    }

    //    return newPos;
    //}
    ///// <summary>
    ///// 强制把新的Boss位置推回Boss房内部
    ///// 如果本来就在Boss房内部 则不改变
    ///// 返回true表示本来就在Boss房内部 位置没有改变
    ///// 返回false表示不在Boss房内部 位置被强行拉回了Boss房内部
    ///// </summary>
    ///// <returns></returns>
    //public bool ForcePushPosiitonInBattleZone(ref Vector3 position)
    //{
    //    bool isInBattleZone = true;

    //    float bossLeftX = position.x - _foxBossBoundsSize.x / 2;
    //    float bossRightX = position.x + _foxBossBoundsSize.x / 2;

    //    //这里加一个内边距 防止Boss超出墙壁外 导致观感差的问题
    //    if (bossLeftX <= _leftWallX)//boss超出了左侧的墙壁
    //    {
    //        position.x = _leftWallX + _foxBossBoundsSize.x / 2;
    //        isInBattleZone = false;
    //    }
    //    else if (bossRightX >= _rightWallX)
    //    {
    //        position.x = _rightWallX - _foxBossBoundsSize.x / 2;
    //        isInBattleZone = false;
    //    }
    //    return isInBattleZone;
    //}
    
    public Vector3 GetMinDistanceFromShowPosition(Vector3 targetPosition)
    {
        return _showPointArray.OrderBy(point => Vector3.SqrMagnitude(point.position - targetPosition)).FirstOrDefault().position;

    }

    public Vector3 GetMaxDistanceFromShowPosition(Vector3 targetPosition)
    {
        return _showPointArray.OrderByDescending(point => Vector3.SqrMagnitude(point.position - targetPosition)).FirstOrDefault().position;
    }

    public float GetGroundY()
    {
        return _groundBoxCollider.bounds.center.y + _groundBoxCollider.bounds.size.y / 2;
    }

    public Transform[] GetClonePointArray()
    {
        return _clonePointArray;
    }

    public Transform GetCloneCenterPoint()
    {
        return _cloneCenterPoint;
    }
}
}
