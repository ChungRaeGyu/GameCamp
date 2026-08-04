using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemys;
    [SerializeField] private GameObject bossEnemy;
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
        HeroType temp = selectedHero.Data.HeroType;
        selectedHero.Configure(RandomHeros(temp++));

        Destroy(matchingHero.gameObject);
        occupiedTiles.Remove(matchingCell);
    }

    private bool TryGetPromotionType(HeroType currentType, out HeroType promotedType)
    {
        switch (currentType)
        {
            case HeroType.Normal:
                promotedType = HeroType.Rare;
                return true;
            case HeroType.Rare:
                promotedType = HeroType.Myth;
                return true;
            case HeroType.Myth:
                promotedType = HeroType.Legendary;
                return true;
            default:
                promotedType = HeroType.Legendary;
                return false;
        }
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
            if (bossEnemy == null)
            {
                Debug.LogWarning("Boss Enemy prefab is not assigned.");
                yield break;
            }

            Instantiate(bossEnemy, enemySpawnPoint.position, Quaternion.identity);
            yield break;
            //10번째마다 보스를 넣을 꺼다.
        }
        if (enemys == null || enemys.Length == 0)
        {
            Debug.LogWarning("Normal enemy prefab is not assigned.");
            yield break;
        }

        int enemyIndex = Mathf.Min(GameManager.instance.roundIndex, enemys.Length - 1);
        for (int i = 0; i < spawnCount; i++)
        {
            Instantiate(enemys[enemyIndex], enemySpawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
