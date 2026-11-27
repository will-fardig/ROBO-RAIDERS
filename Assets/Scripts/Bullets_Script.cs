using System.Collections.Generic;
using UnityEngine;

public class Bullets_Script : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;      // used only if the gun doesn't set one
    [SerializeField] private float _lifetime = 3.0f;
    [SerializeField] private LayerMask wallLayers;     // set to Walls
    private Rigidbody2D _rb;
    [SerializeField] private Spaceman _spaceman;

    // Set by the gun when fired. -1 = not set, fall back to the player's current weapon.
    private float _damage = -1f;
    private int _pierceLeft = 0;                                  // extra enemies it can still pass through
    private readonly HashSet<EnemyHealth> _alreadyHit = new HashSet<EnemyHealth>();

    // Called by Spaceman.Shoot() right after the bullet is created
    public void Init(float speed, float damage, int pierce = 0)
    {
        _speed = speed;
        _damage = damage;
        _pierceLeft = Mathf.Max(0, pierce);
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spaceman = FindAnyObjectByType<Spaceman>();
        Destroy(gameObject, _lifetime); //destroy after a certain time
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = transform.right * _speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit a wall: stop here
        if (((1 << collision.gameObject.layer) & wallLayers) != 0)
        {
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponentInParent<EnemyHealth>();
            if (enemy == null)
            {
                Debug.LogError("Enemy collider has no EnemyHealth component!", collision.gameObject);
                return;
            }
            if (enemy.IsDead) return;
            if (!_alreadyHit.Add(enemy)) return;   // never hit the same enemy twice

            float damage = _damage >= 0f ? _damage : _spaceman.prime_wpn_damage * _spaceman.wpn_damage_multiplier;
            bool killed = enemy.TakeHit(damage, transform.right);
            _spaceman.AddScrap(10 * _spaceman.wpn_damage_multiplier * _spaceman.point_multiplier);
            if (killed) _spaceman.AddScrap(100 * _spaceman.point_multiplier);

            // Pierce: keep flying through to the next enemy
            if (_pierceLeft > 0)
            {
                _pierceLeft--;
                return;
            }
            Destroy(gameObject);
        }
    }
}