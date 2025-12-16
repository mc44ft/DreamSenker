using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorTrapController2 : MonoBehaviour
{
    [SerializeField] private GameObject _teleportExit;
    [SerializeField] private Sprite _teleportDoorSprite;

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
        _teleportExit.SetActive(false);
    }
    private void OnOnlocked()
    {
        _onlockedCount++;
        if (_onlockedCount == _mirrorTraps.Length)
        {
            //全部机关解锁完毕
            foreach (var trap in _mirrorTraps)
            {
                trap.SetSpriteToTeleportDoor(_teleportDoorSprite);
            }
            _teleportExit.SetActive(true);

        }
    }
}
