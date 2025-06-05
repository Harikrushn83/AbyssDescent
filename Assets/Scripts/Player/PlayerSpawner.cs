using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerObject; // Now using a scene object
    [SerializeField] private RoomGenerator _roomGenerator;
    private bool _triedSpawning = false;

    private void Update()
    {
        if (!_triedSpawning && _roomGenerator != null && _roomGenerator.GenerationComplete)
        {
            SpawnPlayer();
            _triedSpawning = true;
        }
    }

    public void SpawnPlayer()
    {
        if (_playerObject == null)
        {
            Debug.LogError("Player object not assigned in hierarchy!");
            return;
        }

        Room startRoom = _roomGenerator.GetStartRoom();

        if (startRoom != null)
        {
            Vector2 spawnPos = startRoom.position;
            _playerObject.transform.position = spawnPos;
            //Debug.Log($"Player moved to {spawnPos} in room size {startRoom.width}x{startRoom.height}");
        }
        else
        {
            Debug.LogError("Start room not found after generation complete!");
        }
    }
}
