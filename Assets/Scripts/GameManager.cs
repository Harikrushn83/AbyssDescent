using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject startUI;
    public GameObject gameOverUI;
    public GameObject winUI;

    private bool gameStarted = false;

    public int totalKills = 0;
    [SerializeField] private TextMeshProUGUI killText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            killText.gameObject.SetActive(false);
            Time.timeScale = 0f; // Paused initially
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        killText.text = "Kills: " + totalKills;
    }

    public void StartGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        killText.gameObject.SetActive(true);
        startUI.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverUI.SetActive(true);
    }

    public void WinGame()
    {
        Time.timeScale = 0f;
        winUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void EndlessMode()
    {
        winUI.SetActive(false);
        Time.timeScale = 1f;
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StartEndlessMode();
        }
    }

    public void RegisterKill()
    {
        totalKills++;
        //Debug.Log("Kills: " + totalKills);
    }

}
