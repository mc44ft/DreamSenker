using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSenker.Shared;

namespace DreamSenker.Traps
{
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MirrorTrap : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _stonetabletSR;
    [SerializeField] private Sprite _needStonetabletSprite;
    
    [SerializeField] private SpriteRenderer _teleportDoorSR;
    [SerializeField] private Sprite _needTeleportDoorSprite;

    private Action _onOnlocked;
    private bool _isTriggered;
    //脏代码 玩家先触发一次才能解锁触发 为了解决场景还未过渡机关就误解锁的问题
    private bool _enableTrigger;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            _enableTrigger = true;
        }
        if (collision.gameObject.CompareTag(Settings.MonsterAmmoTag) && !_isTriggered && _enableTrigger)
        {
            _stonetabletSR.sprite = _needStonetabletSprite;
            _teleportDoorSR.sprite = _needTeleportDoorSprite;
            _onOnlocked?.Invoke();
            _isTriggered = true;
        }
    }
    public void SetCallback(Action onOnloaded)
    {
        _onOnlocked = onOnloaded;   
    }
    public void SetSpriteToTeleportDoor(Sprite sprite)
    {
        _teleportDoorSR.sprite = sprite;
    }
}
}
