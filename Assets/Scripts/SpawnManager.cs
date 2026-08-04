using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemys;
    [SerializeField] private EnemySO[] enemyDatas;
    [SerializeField] private GameObject bossEnemy;
    [SerializeField] private EnemySO[] bossEnemyData;
    [SerializeField] private GameObject commander;
    [SerializeField] private HeroSO[] normalHeros;
    [SerializeField] private HeroSO[] rareHeros;
    [SerializeField] private HeroSO[] mythHeros;
    [SerializeField] private HeroSO[] legendaryHeros;
    [SerializeField] private GameObject heroPrefabs;


    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private Transform commanderSpawnPoint;
    [SerializeField] private int spawnCount;

    SpawnUnit spawnInput;
    [SerializeField] private Tilemap tilemap;
    private Vector3Int cellPos;

    private Dictionary<Vector3Int, Hero> occupiedTiles = new();
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
        if (GameManager.instance.IsGameEnded)
        {
            return;
        }

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
            Vector3 pos = tilemap.GetCellCenterWorld(cellPos);

            if (occupiedTiles.ContainsKey(cellPos))
            {
                GameManager.instance.uiManager.PromotionActive(pos);
                return;
            }
            if (GameManager.instance.moneyManager.GetGold() < 40)
            {
                Debug.Log("골드 부족");
                return;
            }
            GameManager.instance.uiManager.SpawnButtonActive(pos);

        }
    }

    public void SpawnHeroButton()
    {
        //Button 클릭 시 호출되는 함수
        Vector3 pos = tilemap.GetCellCenterWorld(cellPos);

        GameObject obj = Instantiate(heroPrefabs, new Vector3(pos.x,pos.y,0), Quaternion.identity);
        Hero hero = obj.GetComponent<Hero>();
        hero.Configure(RandomHeros(HeroType.Random));
        occupiedTiles.Add(cellPos, hero);
        hero.tilePos = cellPos;
    }

    public void PromotionButton()
    {
        Hero selectedHero = occupiedTiles[cellPos];

        if (selectedHero.Data.HeroType == HeroType.Legendary) return;

        Vector3Int matchingCell = default;
        Hero matchingHero = null;
        foreach (KeyValuePair<Vector3Int, Hero> occupiedTile in occupiedTiles)
        {
            if (occupiedTile.Key == cellPos || occupiedTile.Value.Data != selectedHero.Data)
            {
                continue;
            }

            matchingCell = occupiedTile.Key;
            matchingHero = occupiedTile.Value;
            break;
        }

        if (matchingHero == null)
        {
            return;
        }
        HeroType nextTier = selectedHero.Data.HeroType + 1;
        selectedHero.Configure(RandomHeros(nextTier));

        Destroy(matchingHero.gameObject);
        occupiedTiles.Remove(matchingCell);
    }

    public void DestroyRandomHero()
    {
        List<Vector3Int> heroCells = new();
        foreach (KeyValuePair<Vector3Int, Hero> occupiedTile in occupiedTiles)
        {
            if (occupiedTile.Value != null)
            {
                heroCells.Add(occupiedTile.Key);
            }
        }

        if (heroCells.Count == 0)
        {
            return;
        }

        Vector3Int selectedCell = heroCells[Random.Range(0, heroCells.Count)];
        Destroy(occupiedTiles[selectedCell].gameObject);
        occupiedTiles.Remove(selectedCell);
    }


    private HeroSO RandomHeros(HeroType type)
    {
        switch (type)
        {
            case HeroType.Normal:
                return normalHeros[Random.Range(0, normalHeros.Length)];
            case HeroType.Rare:
                return rareHeros[Random.Range(0, rareHeros.Length)];
            case HeroType.Myth:
                return mythHeros[Random.Range(0, mythHeros.Length)];
            case HeroType.Legendary:
                return legendaryHeros[Random.Range(0, legendaryHeros.Length)];
            case HeroType.Random:
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
            default:return null;
        }
    }


    private void OnNextRound()
    {
        StartCoroutine(CSpawn());
    }

    IEnumerator CSpawn()
    {
        if (GameManager.instance.IsBossRound)
        {
            GameObject spawnedBoss = Instantiate(bossEnemy, enemySpawnPoint.position, Quaternion.identity);
            EnemySO bossData = bossEnemyData != null && bossEnemyData.Length > 0
                ? bossEnemyData[Mathf.Min((GameManager.instance.roundIndex + 1) / 10 - 1, bossEnemyData.Length - 1)]
                : null;
            ConfigureEnemy(spawnedBoss, bossData);
            GameManager.instance.NotifyWaveSpawnComplete();
            yield break;
            //10번째마다 보스를 넣을 꺼다.
        }

        int enemyIndex = Mathf.Min(GameManager.instance.roundIndex, 29);
        for (int i = 0; i < spawnCount; i++)
        {
            Debug.Log("인덱스" + enemyIndex);
            GameObject spawnedEnemy = Instantiate(enemys, enemySpawnPoint.position, Quaternion.identity);
            ConfigureEnemy(spawnedEnemy, enemyDatas[enemyIndex]);
            yield return new WaitForSeconds(0.5f);
        }

        GameManager.instance.NotifyWaveSpawnComplete();
    }

    private void ConfigureEnemy(GameObject enemyObject, EnemySO enemyData)
    {
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null && enemyData != null)
        {
            enemy.Configure(enemyData);
        }
    }
}
