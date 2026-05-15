using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSeeker.Combat.Health;

namespace DreamSeeker.Traps.Buffs
{
public class MournfulGazeDebuff : MonoBehaviour
{
    private float _countdown = 3f;
    private float _tickInterval = 1f;

    private IDamageable _damageable;
    private void Start()
    {
        _damageable = GetComponent<IDamageable>();
        StartCoroutine(Debuff());
    }
    private IEnumerator Debuff()
    {
        while (_countdown > 0)
        {
            yield return new WaitForSeconds(_tickInterval);

            if( _damageable != null)
            {
                _damageable.TakeDamage(2, Vector2.zero);
            }

            _countdown -= _tickInterval;
        }
        Destroy(this);
    }
}
}
