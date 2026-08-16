using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DreamSeeker.Combat.Health;
using DreamSeeker.Framework.Events;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
public class FoxTwoSkillCloneState : StateBase<FoxTwoController>
{
    private List<FoxOneController> _cloneFoxList = new List<FoxOneController>();
    private AudioSource _audioSource;
    public FoxTwoSkillCloneState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        _audioSource = _controller.gameObject.AddComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.volume = AudioManager.Instance.SoundVolume;
        _audioSource.clip = GameResources.Instance.FoxTwoRestoreHealthClip;
        _audioSource.Play();

        EventBus.Subscribe<GameBossDiedEvent>(OnBossDead);

        Debug.Log("进入克隆状态");
        _controller.StartCoroutine(CloneRoutine());
    }

    private IEnumerator CloneRoutine()
    {
        //中心点位置
        Transform cloneCenterPoint = _controller.FoxBossRoom.GetCloneCenterPoint();
        //如果是隐匿状态 那么先现身
        if (_controller.IsHiding)
        {
            //非连段
            yield return _controller.StartShow(cloneCenterPoint.position);
        }
        else
        {
            //连段
            yield return _controller.Rigidbody.DOMove(cloneCenterPoint.position, 0.5f).WaitForCompletion();
        }
        //自身悬浮
        _controller.StartFloating();

        Transform[] clonePointArray = _controller.FoxBossRoom.GetClonePointArray();
        Vector3[] clonePositionArray = new Vector3[clonePointArray.Length];
        //自行值拷贝一下
        for(int i = 0; i < clonePointArray.Length; i++)
        {
            clonePositionArray[i] = clonePointArray[i].position;
        }

        //打乱数组
        clonePositionArray.Shuffle();
        foreach(Vector3 position in clonePositionArray)
        {
            yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillCloneObjectShowIntervalTime);

            FoxOneController cloneController =
                PoolManager.Instance.Pull<FoxOneController>(_controller.FoxTwoConfig.SkillCloneObjectPrefab);
            //初始化克隆体
            cloneController.InitializeAsClone(_controller.TargetPlayer, _controller.FoxBossRoom, position);
            _cloneFoxList.Add(cloneController);
        }
        //保持分身
        yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillCloneObjectKeepTime);

        //分身消散
        //打乱列表
        //视分身数量加血
        _controller.Health.RestoreHealth(_cloneFoxList.Count * _controller.FoxTwoConfig.SkillCloneSingleRestoreHealthAmount);

        _cloneFoxList.Shuffle();

        //创建一个数组副本 用于解决在遍历克隆体的时候 克隆体被击杀的错误
        var cloneSnapshot = _cloneFoxList.ToArray();
        foreach (FoxOneController cloneObject in cloneSnapshot)
        {
            //检查现在的clone体是否还在主列表中 如果不在直接跳过
            if (!_cloneFoxList.Contains(cloneObject))
                continue;

            yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillCloneDisappearIntervalTime);

            //二次检查
            if (!_cloneFoxList.Contains(cloneObject))
                continue;

            yield return cloneObject.StartHide();

            if(cloneObject != null && cloneObject.gameObject.activeInHierarchy)
            {
                cloneObject.StopFloating();
                cloneObject.PushSelfToPool();
            }

            //保持和主列表同步
            if (_cloneFoxList.Contains(cloneObject))
            {
                _cloneFoxList.Remove(cloneObject);
            }
            
        } 
        //取消自身悬浮
        _controller.StopFloating();
        _machineManager.TransitionTo(_controller.HideState);
    }
    public override void Exit()
    {
        EventBus.Unsubscribe<GameBossDiedEvent>(OnBossDead);
        //恢复Boss状态
        _controller.transform.rotation = Quaternion.identity;
        _controller.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _controller.ResetSkillCooldownTimer(_controller.SkillCloneState);


        GameObject.Destroy(_audioSource);
    }
    /// <summary>
    /// 移除已死亡的狐狸克隆体并在全部清除后恢复本体状态。
    /// </summary>
    private void OnBossDead(GameBossDiedEvent eventData)
    {
        if (eventData.BossType == EBossType.FoxClone &&
            eventData.BossGameObject.TryGetComponent(out FoxOneController cloneController) &&
            _cloneFoxList.Contains(cloneController))
        {
            _cloneFoxList.Remove(cloneController);
            if (_cloneFoxList.Count == 0)
            {
                _controller.StopFloating();
                if (_controller.FaceLeft == 1)
                {
                    _controller.transform.Rotate(0, 0, -90);
                }
                else
                {
                    _controller.transform.Rotate(0, 0, 90);
                }
                _controller.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
                _controller.Rigidbody.gravityScale = 5f;
            }
        }
    }
    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
        
    }
}
}
