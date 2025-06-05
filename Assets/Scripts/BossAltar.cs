using UnityEngine;

public class BossAltar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject stoneStatue;
    [SerializeField] private GameObject bossPrefab;

    [Header("Wave Settings")]
    public int bossWaveIndex; // Set by RoomGenerator when activating boss wave

    [HideInInspector] public Room bossRoom;
    private bool activated;

    public void ActivateAltar()
    {
        //Debug.Log("activate alter");
        stoneStatue.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || activated) return;

        activated = true;
        RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();

        if (roomGenerator != null)
        {
            roomGenerator.ActivateBarriers(this);
        }

        SpawnBoss();
        stoneStatue.SetActive(false);
        //Destroy(gameObject);
    }

    void SpawnBoss()
    {
        GameObject boss = Instantiate(bossPrefab, transform.position, Quaternion.identity);
        BossAI bossAI = boss.GetComponent<BossAI>();

        if (bossAI != null)
        {
            bossAI.connectedAltar = this;
            bossAI.currentWaveIndex = this.bossWaveIndex; // Critical wave progression hook

            // Immediate wave scaling (optional visual feedback)
            bossAI.transform.localScale *= (1 + bossWaveIndex * 0.1f); // 10% size increase per wave
        }
    }

    // Visual debug
    //void OnDrawGizmos()
    //{
    //    if (bossRoom != null)
    //    {
    //        Gizmos.color = Color.magenta;
    //        Gizmos.DrawWireCube(bossRoom.position, new Vector3(bossRoom.width, bossRoom.height, 0));
    //        GUI.color = Color.white;
    //        UnityEditor.Handles.Label(transform.position, $"Wave {bossWaveIndex + 1}");
    //    }
    //}
}