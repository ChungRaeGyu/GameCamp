using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform[] waypoints;
    public bool isStart = false;
    public event Action nextRound;
    public int roundIndex = 0;
    public SpawnManager spawnManager;
    public UiManager uiManager;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void GameStart()
    {
        isStart = true;
        nextRound?.Invoke();
    }
    public void StageClear()
    {
        roundIndex++;
    }
    public void GameOver()
    {
        //roundIndex가 한 40라운드에서 게임 종료 하는 걸로;;
    }
}
