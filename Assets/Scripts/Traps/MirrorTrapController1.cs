using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSenker.Traps
{
public class MirrorTrapController1 : MonoBehaviour
{
    [SerializeField] private GameObject _teleportExit;
    [SerializeField] private Sprite _teleportDoorSprite;

    [SerializeField] private SpriteRenderer _mainSR;
    [SerializeField] private float _showOrHideDuration = 1f;
    private MirrorTrap[] _mirrorTraps;
    private int _onlockedCount = 0;
    private void Awake()
    {
        _mirrorTraps = GetComponentsInChildren<MirrorTrap>();
        foreach (var trap in _mirrorTraps)
        {
            trap.SetCallback(OnOnlocked);
        }
    }
    private void Start()
    {
        _mainSR.color = new Color(_mainSR.color.r, _mainSR.color.g, _mainSR.color.b, 0f);
        _teleportExit.SetActive(false);
    }
    private void OnOnlocked()
    {
        _onlockedCount++;
        if(_onlockedCount == _mirrorTraps.Length)
        {
            //全部机关解锁完毕
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_mainSR.DOFade(1f, _showOrHideDuration));
            sequence.AppendInterval(4f);
            sequence.Append(_mainSR.DOFade(0f, _showOrHideDuration));
            sequence.OnComplete(() =>
            {
                foreach (var trap in _mirrorTraps)
                {
                    trap.SetSpriteToTeleportDoor(_teleportDoorSprite);
                }
                _teleportExit.SetActive(true);
            });

        }
    }
}
}
