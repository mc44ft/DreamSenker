using UnityEngine;
using DreamSeeker.Combat.Health;
using DreamSeeker.Shared;

/// <summary>
/// 用于蜘蛛四技能的蛛网检测伤害
/// </summary>

namespace DreamSeeker.Characters.Bosses
{
public class SkillShakeHit : MonoBehaviour
{
    private int m_damage;
    public void UpdateDamage(int damage)
    {
        m_damage = damage;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(m_damage, Vector2.zero);
        }
    }
}
}
