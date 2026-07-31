using System;
using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private GameObject commander;
    [SerializeField] private GameObject[] heros;


    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private Transform commanderSpawnPoint;
    [SerializeField] private int spawnCount;

    private void Start()
    {
        GameManager.instance.nextRound += OnNextRound;
        Instantiate(commander, commanderSpawnPoint.position, Quaternion.identity);
    }

    private void OnNextRound()
    {
        StartCoroutine(CSpawn());
    }

    IEnumerator CSpawn()
    {
        if (GameManager.instance.roundIndex % 10 == 0)
        {
            spawnCount = 1;
            //10번째마다 보스를 넣을 꺼다.
        }
        for (int i = 0; i < spawnCount; i++)
        {
            Instantiate(enemys[GameManager.instance.roundIndex], enemySpawnPoint.position, Quaternion.identity);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        spawnCount = 5;
    }
}
