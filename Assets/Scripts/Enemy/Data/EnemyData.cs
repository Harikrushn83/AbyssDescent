// Assets/Actors/Enemies/Components/EnemyData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab; // Make sure this exists and is assigned

    [Header("Core Stats")]
    public int maxHP = 30;
    public float moveSpeed = 3f;
    public int damage = 10;

    [Header("Behavior")]
    public float aggroRange = 8f;
    public float leashRange = 20f;
    public bool isRanged = false;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public float attackRange = 1.0f;
}