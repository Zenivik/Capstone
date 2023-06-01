using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public GameObject AfterDeathCamera;
    public GameObject GameOverScreen;
    public int TotalEnemySpawned = 0;
    public int MaxEnemySpawned = 5;
    public float TimeBetweenWaves;
    private int AddEnemiesPerWave = 5; 
    
    [HideInInspector]
    public int TotalEnemiesKilled = 0;

    public TMP_Text WaveText;
    private int WaveCount = 1;

    public TMP_Text GameOverText;

    [HideInInspector]
    public bool IsGameOver = false;

    [HideInInspector]
    public GameObject LeftPortal;
    [HideInInspector]
    public Transform LeftPortalTransform;
    [HideInInspector]
    public GameObject RightPortal;
    [HideInInspector]
    public Transform RightPortalTransform;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGameOver) GameOver();

        if(TotalEnemiesKilled >= MaxEnemySpawned)
        {
            WaveCount++;
            WaveText.text = "Wave " + WaveCount.ToString();
            TimeBetweenWaves = 5f;
            MaxEnemySpawned += AddEnemiesPerWave;
            TotalEnemiesKilled = 0;
            TotalEnemySpawned = 0;
        }

        TimeBetweenWaves -= Time.deltaTime;
    }

    public void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        AfterDeathCamera.SetActive(true);
        GameOverScreen.SetActive(true);
        GameOverText.text = "You Survived " + WaveCount.ToString() + " Rounds and Died";
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
