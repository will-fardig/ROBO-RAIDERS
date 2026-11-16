using UnityEngine;

public class Bullets_Script : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;
    [SerializeField] private float _lifetime = 3.0f;
    private string EnemyTag = "Enemy";
    private Rigidbody2D _rb;
    [SerializeField] private RunnerChase _runnerchase;
    [SerializeField] private Spaceman _spaceman;
    private Coroutine hitCoroutine;
    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            RunnerChase enemy = collision.GetComponent<RunnerChase>();

            if (enemy == null)
            {
                Debug.LogError("Enemy collider has no RunnerChase component!", collision.gameObject);
                return;
            }

            if (enemy.RunnerHealth <= 0) return;
            bool killed = enemy.TakeHit(_spaceman.prime_wpn_damage *_spaceman.wpn_damage_multiplier, transform.right);
            _spaceman.AddScrap(10 * _spaceman.wpn_damage_multiplier * _spaceman.point_multiplier);
            if (killed) _spaceman.AddScrap(100 * _spaceman.point_multiplier);

            Debug.Log("Damage dealt. Remaining health: " + enemy.RunnerHealth);

            Destroy(gameObject);
        }
    }

}
