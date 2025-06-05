using UnityEngine;

public class EnemyAttackHandler : MonoBehaviour
{
    private Transform _player;
    private PlayerHealth _playerHealth;
    private EnemyData _data;
    private float _lastAttackTime;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player").transform;
        _playerHealth = _player.GetComponent<PlayerHealth>();
        _data = GetComponent<EnemyAI>().data; // Assuming all enemies have EnemyAI
    }

    //private void Update()
    //{
    //    if (_player == null || _data == null) return;

    //    float distance = Vector2.Distance(transform.position, _player.position);

    //    if (distance <= _data.attackRange && Time.time >= _lastAttackTime + _data.attackCooldown)
    //    {
    //        Attack();
    //    }
    //}

    //private void Attack()
    //{
    //    if (_playerHealth != null)
    //    {
    //        _playerHealth.TakeDamage(_data.damage);
    //        _lastAttackTime = Time.time;
    //    }
    //}

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_playerHealth == null)
                _playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (_playerHealth != null && Time.time >= _lastAttackTime + _data.attackCooldown)
            {
                _playerHealth.TakeDamage(_data.damage);
                _lastAttackTime = Time.time;
            }
        }
    }
}
