using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform[] waypoints;
    public bool isStart = false;
    public event Action nextRound;
    public int roundIndex = 0;
    [SerializeField] private float normalRoundDuration = 15f;
    [SerializeField] private float bossRoundDuration = 30f;
    [SerializeField] private float clearedWaveDelay = 1f;
    [SerializeField] private int totalRoundCount = 30;
    private float timer;
    private float clearedWaveTimer;
    private bool isWaveSpawnComplete;
    public bool IsGameEnded { get; private set; }
    public SpawnManager spawnManager;
    public UiManager uiManager;
    public MoneyManager moneyManager;
    public EnemyManager enemyManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public bool IsBossRound => (roundIndex + 1) % 10 == 0;

    private void Update()
    {
        if (!isStart)
        {
            return;
        }

        if (isWaveSpawnComplete && enemyManager != null && enemyManager.ActiveEnemies.Count == 0)
        {
            clearedWaveTimer += Time.deltaTime;
            if (clearedWaveTimer >= clearedWaveDelay)
            {
                AdvanceRound();
            }

            return;
        }

        clearedWaveTimer = 0f;

        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            return;
        }

        AdvanceRound();
    }

    private void AdvanceRound()
    {
        roundIndex++;
        if (roundIndex >= totalRoundCount)
        {
            GameClear();
            return;
        }
        uiManager.UpdateStage();

        StartRound();
    }
    public void GameStart()
    {
        if (isStart)
        {
            return;
        }

        isStart = true;
        StartRound();
    }

    private void StartRound()
    {
        isWaveSpawnComplete = false;
        clearedWaveTimer = 0f;
        nextRound?.Invoke();
        timer = IsBossRound ? bossRoundDuration : normalRoundDuration;
    }

    public void NotifyWaveSpawnComplete()
    {
        if (!isStart || IsGameEnded)
        {
            return;
        }

        isWaveSpawnComplete = true;
    }
    public void GameOver()
    {
        EndGame(false);
        //Commander가 죽으면 게임오버
    }
    public void GameClear()
    {
        EndGame(true);
        
        //roundIndex가 한 30라운드에서 게임 종료 하는 걸로;;
    }

    private void EndGame(bool isVictory)
    {
        if (IsGameEnded)
        {
            return;
        }

        IsGameEnded = true;
        isStart = false;
        Time.timeScale = 0f;
        uiManager?.ShowResult(isVictory);
    }
}
