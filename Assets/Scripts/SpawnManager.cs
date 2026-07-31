using System;
using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int spawnCount;

    private void Start()
    {
        GameManager.instance.nextRound += OnNextRound;
    }

    private void OnNextRound()
    {
        StartCoroutine(CSpawn());
    }

    IEnumerator CSpawn()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Instantiate(enemys[GameManager.instance.roundIndex], spawnPoint.position, Quaternion.identity);
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
