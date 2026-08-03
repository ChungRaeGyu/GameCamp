using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private GameObject commander;
    [SerializeField] private GameObject[] normalHeros;
    [SerializeField] private GameObject[] rareHeros;
    [SerializeField] private GameObject[] mythHeros;
    [SerializeField] private GameObject[] legendaryHeros;


    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private Transform commanderSpawnPoint;
    [SerializeField] private int spawnCount;

    SpawnUnit spawnInput;
    [SerializeField] private Tilemap tilemap;
    private Vector3Int cellPos;

    private Dictionary<Vector3Int, GameObject> occupiedTiles = new();
    private void Awake()
    {
        spawnInput = new SpawnUnit();
        spawnInput.Enable();
        spawnInput.KeyBoardMouse.Spawn.canceled += UnitSpawn;
        Debug.Log("완");
    }

    private void Start()
    {
        GameManager.instance.spawnManager = this;
        GameManager.instance.nextRound += OnNextRound;
        Instantiate(commander, commanderSpawnPoint.position, Quaternion.identity);
    }

    private void UnitSpawn(InputAction.CallbackContext context)
    {
        Debug.Log("뭐지)");
        if (context.phase == InputActionPhase.Canceled)
        {

            Debug.Log("Spawn");
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(spawnInput.KeyBoardMouse.MousePos.ReadValue<Vector2>());
            Vector3 worldPos = new Vector3(mousePos.x, mousePos.y, 0);
            // 월드 좌표 → 타일 좌표
            cellPos = tilemap.WorldToCell(worldPos);

            TileBase tile = tilemap.GetTile(cellPos);

            Debug.Log($"타일 좌표: {cellPos}, 타일: {tile}");
            Debug.Log(tilemap.cellBounds);
            if (tile == null)
            {
                Debug.Log("으잉?");
                return;
            }

            if (occupiedTiles.ContainsKey(cellPos))
                return;
            if (GameManager.instance.moneyManager.GetGold() < 40)
            {
                Debug.Log("골드 부족");
                return;
            }
            else
            {
                GameManager.instance.moneyManager.AddGold(-40);
            }
            GameManager.instance.uiManager.SpawnButtonActive(worldPos);

        }
    }

    public void SpawnHeroButton()
    {
        //Button 클릭 시 호출되는 함수
        Vector3 pos = tilemap.GetCellCenterWorld(cellPos);

        GameObject obj = Instantiate(RandomHeros(), pos, Quaternion.identity);

        occupiedTiles.Add(cellPos, obj);

    }

    private GameObject RandomHeros()
    {
        int random = Random.Range(0, 100);

        if (random < 85)
        {
            return normalHeros[Random.Range(0, normalHeros.Length)];
        }
        else if (random < 95)
        {
            return rareHeros[Random.Range(0, rareHeros.Length)];
        }
        else if (random < 99)
        {
            return mythHeros[Random.Range(0, mythHeros.Length)];
        }
        else
        {
            return legendaryHeros[Random.Range(0, legendaryHeros.Length)];
        }
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
