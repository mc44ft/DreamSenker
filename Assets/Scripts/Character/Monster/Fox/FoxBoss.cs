using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Data.ScriptableObjects.Character.Monster.Fox;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]//自己身上的这个碰撞体是用来让玩家攻击检测的
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class FoxBoss : PoolBase, IDamageable, ITouchDamageable
{
    //目标玩家
    public Transform TargetPlayer {  get; private set; }
    [SerializeField] protected FoxBossConfigSO _foxConfig;
    [SerializeField] private Collider2D _touchDamageCollider;//用于触碰伤害的子物体
    [SerializeField] private GameObject _selectedEffect;
    //-------------------- Self Components ----------------------
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator Animator { get; private set; }
    public Health Health { get; private set; }
    public Material Material { get; private set; }
    //-------------------- Public  Parameter ----------------------
    public bool IsHiding { get; private set; }//持有当前是否是隐匿状态的标志
    public IFoxBossRoom FoxBossRoom { get; private set; }
    //默认面朝向为左侧
    public int FaceLeft { get; private set; } = 1;

    private Coroutine _getHitColorChangeCoroutine;
    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        Health = GetComponent<Health>();
        //这一步会自动Clone一份 不会影响主体材质
        Material = SpriteRenderer.material;
        
    }
    public virtual void Initialize(Transform player, IFoxBossRoom foxBossRoom)
    {
        FoxBossRoom = foxBossRoom;
        TargetPlayer = player;
    }
    public Coroutine StartHide(bool isCustomDuration = false, float duration = 0f)
    {
        return StartCoroutine(HidingRoutine(isCustomDuration, duration));
    }
    public Coroutine StartShow(Vector3 showPosition)
    {
        return StartCoroutine(ShowingRoutine(showPosition));
    }
    private IEnumerator HidingRoutine(bool isCustomDuration, float duration)
    {
        //Boss处于隐匿状态
        IsHiding = true;
        //消失时关闭碰撞（身体处于虚无状态） 避免影响玩家
        SetBossTouchDamage(false);
        //隐匿 并等待隐匿动画结束
        yield return HideMe(isCustomDuration, duration).WaitForCompletion();//这是DOTween内部的一个方法 专门为了协程中等待动画结束设计的
    }
    private IEnumerator ShowingRoutine(Vector3 showPosition)
    {
        //将Boss设置到现身点位
        transform.position = showPosition;
        //更新Boss朝向
        UpdateFace();
        
        //现身
        yield return ShowMe().WaitForCompletion();
        //开启Boss触碰伤害
        SetBossTouchDamage(true);
        //Boss隐匿状态结束
        IsHiding = false;
    }
    /// <summary>
    /// 设置Boss触碰伤害的开关
    /// </summary>
    /// <param name="isOpen"></param>
    private void SetBossTouchDamage(bool isOpen)
    {
        _touchDamageCollider.enabled = isOpen;
    }

    private TweenerCore<float, float, FloatOptions> HideMe(bool isCustomDuration, float duration)
    {
        if (isCustomDuration)
        {
            return Material.DOFloat(1f, Settings.DissolveAmountString, duration).
                SetEase(Ease.InOutQuad);
        }
        else
        {
            return Material.DOFloat(1f, Settings.DissolveAmountString, _foxConfig.HideDuration).
                SetEase(Ease.InOutQuad);
        }
            
    }
    private TweenerCore<float, float, FloatOptions> ShowMe()
    {
        return Material.DOFloat(0f, Settings.DissolveAmountString, _foxConfig.ShowDuration).
            SetEase(Ease.InOutQuad);
    }
    
    /// <summary>
    /// 现身时调用
    /// </summary>
    public void UpdateFace()
    {
        if (transform.position.x - TargetPlayer.position.x > 0)
        {
            //更新面朝向为左侧
            FaceLeft = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            //更新面朝向为右侧
            FaceLeft = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    public void StartFloating()
    {
        transform.DOMoveY(transform.position.y + _foxConfig.FloatingOffset, _foxConfig.FloatingOneLoopTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    public void StopFloating()
    {
        transform.DOKill();
    }
    public override void OnPull()
    {
        
    }

    public override void OnPush()
    {
        
    }
    

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void Select()
    {
        _selectedEffect.SetActive(true);
    }
    public void Deselect()
    {
        _selectedEffect.SetActive(false);
    }
    public virtual void TakeDamage(int damage, Vector2 attackDirection)
    {
        if(Health.IsDead) return;   
        Health.ApplyDamage(damage);

        if(_getHitColorChangeCoroutine != null)
        {
            StopCoroutine(GetHitColorChangeRoutine());
            _getHitColorChangeCoroutine = null;
        }
        _getHitColorChangeCoroutine = StartCoroutine(GetHitColorChangeRoutine());
    }
    private IEnumerator GetHitColorChangeRoutine()
    {
        SpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        SpriteRenderer.color = Color.white;
    }
    public int GetTouchDamage()
    {
        return _foxConfig.TouchDamage;
    }
}
