using UnityEngine;

// Hurts the player on touch. Leave it off enemies that shouldn't (e.g. ranged ones).
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    public int Damage => damage;
}