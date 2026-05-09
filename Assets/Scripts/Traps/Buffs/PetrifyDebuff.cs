
using UnityEngine;

using DreamSenker.Managers;

namespace DreamSenker.Traps.Buffs
{
[DisallowMultipleComponent]
public class PetrifyDebuff : MonoBehaviour
{
    private float _duration;
    private SpriteRenderer _spriteRenderer;
    public void Initialize(float duration)
    {
        _duration = duration;

        //这里获取的是玩家本身的SpriteRenderer
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = Color.gray;

        InputManager.Instance.SetPlayerInputAction(false);
    }
    private void Update()
    {
        _duration -= Time.deltaTime;
        if(_duration <= 0)
        {
            InputManager.Instance.SetPlayerInputAction(true);

            _spriteRenderer.color = Color.white;
            Destroy(this);
        }
    }
}
}
