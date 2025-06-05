using UnityEngine;
using Aoiti.Pathfinding;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Combat Settings")]
    public float moveSpeed = 3f;
    public int maxHP = 30;

    [Header("Behavior Settings")]
    public float aggroRange = 8f;
    public float leashRange = 20f;

    [Header("Pathfinding Settings")]
    [SerializeField] LayerMask obstacleMask = ~0;
    [SerializeField] LayerMask walkableMask = ~0;

    public EnemyData data;

    private Transform _player;
    public float _currentHP;
    private Pathfinder<Vector3> _pathfinder;
    private List<Vector3> _path = new List<Vector3>();
    private Rigidbody2D _rb; // Cached component
    [HideInInspector] public EnemySpawner spawner;
    public EnemyDropSystem dropSystem;

    public float DebugCurrentHP => _currentHP;

    [Header("Boss Settings")]
    public bool isBoss;

    private bool isDead = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>(); // Cache once
        _currentHP = data.maxHP;
        moveSpeed = data.moveSpeed;
        aggroRange = data.aggroRange;
        leashRange = data.leashRange;
        dropSystem = EnemyDropSystem.Instance;
        _pathfinder = new Pathfinder<Vector3>(HeuristicDistance, ConnectedNodesAndStepCosts, 100);
    }

    void Update()
    {
        if (_player == null)
        {
            _player = GameObject.FindWithTag("Player").transform;
        }

        if (Vector2.Distance(transform.position, _player.position) > leashRange)
        {
            gameObject.SetActive(false);
            spawner.ReturnEnemyToPool(gameObject);
            return;
        }

        if (Vector2.Distance(transform.position, _player.position) <= aggroRange)
        {
            bool hasDirectPath = !Physics2D.Linecast(transform.position, _player.position, obstacleMask);

            if (hasDirectPath || (_path.Count > 0 && Vector2.Distance(_player.position, _path[_path.Count - 1]) > 1.5f))
            {
                _path.Clear();
            }

            if (_path.Count > 0 && Vector2.Distance(transform.position, _path[0]) < 0.1f)
            {
                if (Vector2.Distance(transform.position, _path[0]) < 0.05f)
                {
                    _path.RemoveAt(0);
                }
            }
            if (hasDirectPath && Vector2.Distance(transform.position, _player.position) < 1f)
            {
                Vector2 newPos = Vector2.MoveTowards(
                    transform.position,
                    _player.position,
                    moveSpeed * Time.deltaTime
                );
                _rb.MovePosition(newPos);
            }
            else if (_path.Count == 0)
            {
                Vector3 snappedPos = new Vector3(
                    Mathf.Round(transform.position.x),
                    Mathf.Round(transform.position.y),
                    0
                );
                Vector3 snappedPlayerPos = new Vector3(
                    Mathf.Round(_player.position.x),
                    Mathf.Round(_player.position.y),
                    0
                );

                if (_pathfinder.GenerateAstarPath(snappedPos, snappedPlayerPos, out _path) && _path.Count > 0)
                {
                    Vector2 newPos = Vector2.MoveTowards(
                        transform.position,
                        _path[0],
                        moveSpeed * Time.deltaTime
                    );
                    _rb.MovePosition(newPos);

                    if (Vector2.Distance(transform.position, _path[0]) < 0.1f)
                        _path.RemoveAt(0);
                }
            }
        }
        if (_path.Count > 0)
        {
            if (Vector2.Distance(transform.position, _path[0]) < 0.1f)
            {
                _path.RemoveAt(0);
                if (_path.Count == 0) return;
            }

            Vector2 newPos = Vector2.MoveTowards(
                transform.position,
                _path[0],
                moveSpeed * Time.deltaTime
            );
            _rb.MovePosition(newPos);
        }
        Debug.Log("hp" + _currentHP);
        Debug.Log("speed" + moveSpeed);
    }

    float HeuristicDistance(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude;
    }

    Dictionary<Vector3, float> ConnectedNodesAndStepCosts(Vector3 center)
    {
        Dictionary<Vector3, float> neighbors = new Dictionary<Vector3, float>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3 neighbor = new Vector3(
                    Mathf.Round(center.x + x),
                    Mathf.Round(center.y + y),
                    0
                );

                if (!Physics2D.Linecast(center, neighbor, obstacleMask))
                {
                    Vector3 direction = neighbor - center;
                    float cost = direction.magnitude;

                    if (Mathf.Abs(x * y) == 1)
                    {
                        bool nearWall = Physics2D.OverlapPoint(center + new Vector3(x, 0, 0), obstacleMask) ||
                                      Physics2D.OverlapPoint(center + new Vector3(0, y, 0), obstacleMask);
                        if (nearWall) cost *= 1.3f;
                    }

                    neighbors.Add(neighbor, cost);
                }
            }
        }
        return neighbors;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        _currentHP -= damage;

        if (_currentHP <= 0)
        {
            isDead = true;
            GameManager.Instance.RegisterKill();
            if (dropSystem != null)
            {
                dropSystem.HandleEnemyDeath(transform.position);
            }
            gameObject.SetActive(false);
            //ResetEnemy();
            spawner?.ReturnEnemyToPool(gameObject); // Safe null check
        }
    }

    public void ResetEnemy()
    {
        isDead = false;
        if (!spawner.isEndlessMode)
        {
            _currentHP = data.maxHP;
        }
        _path.Clear(); // Also clear any old pathfinding data
    }

    void OnDrawGizmos()
    {
        if (_path == null || _path.Count == 0) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < _path.Count - 1; i++)
        {
            Gizmos.DrawLine(_path[i], _path[i + 1]);
        }
    }
}
