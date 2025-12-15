using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Spawn Configuration")]
    [SerializeField] private float minSpawnX = 3f;
    [SerializeField] private float maxSpawnX = 18f;
    [SerializeField] private float minSpawnZ = -3f;
    [SerializeField] private float maxSpawnZ = -8f;
    
    [Header("Game Objects")]
    [SerializeField] private Transform runner;
    [SerializeField] private Transform chaser;
    [SerializeField] private Transform runnerSpawnPoint;
    [SerializeField] private Transform chaserSpawnPoint;
    [SerializeField] private float gameDuration = 10f;

    [Header("Display")]
    [SerializeField] private TextMeshPro timeText;
    [SerializeField] private TextMeshPro iterationText;
    [SerializeField] private TextMeshPro runnerScoreText;
    [SerializeField] private TextMeshPro chaserScoreText;
    
    private float gameTimer;
    private int iterationCount = 0;
    private int runnerScore = 0;
    private int chaserScore = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        if (gameTimer > 0)
        {
            gameTimer -= Time.deltaTime;
            timeText.text = gameTimer.ToString("F2") + " s";
        }
    }

    public void ResetGame()
    {
        // Randomize spawn positions.
        runnerSpawnPoint.localPosition = new Vector3(
            Random.Range(minSpawnX, maxSpawnX),
            runnerSpawnPoint.localPosition.y,
            Random.Range(minSpawnZ, maxSpawnZ)
        );
        chaserSpawnPoint.localPosition = new Vector3(
            Random.Range(minSpawnX, maxSpawnX),
            runnerSpawnPoint.localPosition.y,
            Random.Range(minSpawnZ, maxSpawnZ)
        );
        // Reset player positions.
        runner.SetPositionAndRotation(runnerSpawnPoint.position, runnerSpawnPoint.rotation);
        chaser.SetPositionAndRotation(chaserSpawnPoint.position, chaserSpawnPoint.rotation);
        runner.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        chaser.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        // Reset the game timer.
        gameTimer = gameDuration;
        // Increment iteration count and update display.
        iterationText.text = iterationCount.ToString();
    }
    
    public bool CheckTimeOut()
    {
        if (gameTimer <= 0)
        {
            Debug.Log("Runner survived! Game Over.");
            runnerScore++;
            runnerScoreText.text = runnerScore.ToString();
            iterationCount++;
            return true;
        }
        return false;
    }

    public void RunnerCaught()
    {
        Debug.Log("Chaser caught the Runner! Game Over.");
        chaserScore++;
        chaserScoreText.text = chaserScore.ToString();
        iterationCount++;
    }
    
    public float GetTimeLeft()
    {
        return gameTimer;
    }
}
