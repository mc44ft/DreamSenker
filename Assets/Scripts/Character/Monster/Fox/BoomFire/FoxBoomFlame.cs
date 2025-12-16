using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class FoxBoomFlame : PoolBase
{
    [SerializeField] private FoxBoomFireConfigSO _config;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private bool _enableBoom;
    //记录已经攻击过的对象
    private IDamageable hitObject;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();


        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = AudioManager.Instance.SoundVolume - 0.5f;
        audioSource.clip = GameResources.Instance.FoxFlameBurnClip;
        audioSource.Play();
    }
    
    private void Start()
    {
        Sequence sequence = DOTween.Sequence();
        //动画加速
        sequence.Insert(0, DOVirtual.Float(
            1, 
            _config.AnimationMaxSpeed, 
            Random.Range(_config.DetonateTime - _config.DetonateTimeOffsetRange, _config.DetonateTime + _config.DetonateTimeOffsetRange), 
            (value) =>
                {
                _animator.SetFloat("AnimSpeed", value);
                })).
            SetEase(Ease.InQuad);

        //剧烈抖动
        float shakeStartTime = _config.DetonateTime - _config.AnimationShakeStartTimeDown;
        if (shakeStartTime > 0)
        {
            sequence.Insert(shakeStartTime, transform.DOShakePosition(1, 0.2f, 20, 90));
        }
        sequence.AppendCallback(() =>
        {
            _enableBoom = true;
        });
        //引爆
        sequence.AppendCallback(() =>
        {
            transform.DOScale(_config.BoomScaleMultiply, _config.BoomScaleDuration).SetEase(Ease.OutBack).
                OnComplete(() =>
                {
                    _enableBoom = false;
                });
        });
        //余波消散
        sequence.Append(_spriteRenderer.DOFade(0f, _config.BoomFadeToZeroDuration));
        //结束
        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });

    }
    public void Launch(Vector3 endPosition,float power,  float duration)
    {
        transform.DOJump(endPosition, power, 1, duration).SetEase(Ease.OutQuad);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag) && _enableBoom)
        {
            if(collision.TryGetComponent(out IDamageable damageable) && damageable != hitObject)
            {
                damageable.TakeDamage(_config.BoomDamage, Vector2.zero);
                hitObject = damageable;
            }
        }
    }


    private void OnDestroy()
    {
        transform.DOKill();
        _spriteRenderer.DOKill();
    }

    public override void OnPull()
    {
        
    }

    public override void OnPush()
    {
        
    }
}
