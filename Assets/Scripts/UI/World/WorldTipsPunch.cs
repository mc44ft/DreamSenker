using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSeeker.UI.World
{
public class WorldTipsPunch : MonoBehaviour
{
    [Header("BASIC DETAILS")]
    [SerializeField] private Sprite _keyboardSprite;
    [SerializeField] private Sprite _gameControllerSprite;
    [SerializeField] private Image _image;
    [Header("PUNCH EFFECT")]
    [SerializeField] private PunchInfo[] _punchInfoArray;
    [Tooltip("循环停顿时间")]
    [SerializeField] private float _whileIntervalTime = 0.05f;
    

    private void Start()
    {
        Sequence sequence = DOTween.Sequence();

        foreach (var info in _punchInfoArray)
        {
            sequence.Append(transform.DOScale(0.01f * info.Strength, info.PunchDuration).SetEase(info.PunchEaseCurve));
            //加个停顿
            sequence.AppendInterval(info.PunchIntervalTime);
        }
        //加个停顿
        sequence.AppendInterval(_whileIntervalTime);
        //设置无限循环
        sequence.SetLoops(-1);
        //把序列和游戏对象绑定 随游戏对象一起消亡
        sequence.SetLink(gameObject);
    }
    [Serializable]
    private class PunchInfo
    {
        [Tooltip("放缩时间")]
        public float PunchDuration = 0.1f;
        [Tooltip("放缩倍数")]
        public float Strength = 1.2f;
        [Tooltip("放缩停顿时间")]
        public float PunchIntervalTime = 0.05f;
        [Tooltip("放缩曲线")]
        public Ease PunchEaseCurve;
    }
}

}
