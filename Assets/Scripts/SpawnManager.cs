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

    [SerializeField] private EnemySO[] missionEnemyDatas;

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
    [SerializeField] private List<TileBase> nonSpawnableTiles = new();
    [SerializeField, Min(1)] private int attackBuffTileCount = 2;
    [SerializeField, Min(1f)] private float attackBuffDamageMultiplier = 2f;
    [SerializeField] private List<Vector3Int> attackBuffTiles = new();
    private readonly List<AttackBuffTileGlow> attackBuffTileGlows = new();
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
        SelectAttackBuffTiles();
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

            Debug.Log(tilemap.cellBounds);
            if (tile == null)
            {
                return;
            }

            if (nonSpawnableTiles.Contains(tile))
            {
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
        hero.SetTileAttackMultiplier(attackBuffTiles.Contains(cellPos) ? attackBuffDamageMultiplier : 1f);
        occupiedTiles.Add(cellPos, hero);
        hero.tilePos = cellPos;
    }

    private void SelectAttackBuffTiles()
    {
        attackBuffTiles.Clear();

        List<Vector3Int> candidates = new();
        foreach (Vector3Int tilePosition in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile != null && !nonSpawnableTiles.Contains(tile))
            {
                candidates.Add(tilePosition);
            }
        }

        int selectedCount = Mathf.Min(attackBuffTileCount, candidates.Count);
        for (int i = 0; i < selectedCount; i++)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            attackBuffTiles.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }

        CreateAttackBuffTileGlows();
        Debug.Log($"Selected {attackBuffTiles.Count} attack buff tile(s): {string.Join(", ", attackBuffTiles)}");
    }

    private void CreateAttackBuffTileGlows()
    {
        foreach (AttackBuffTileGlow glow in attackBuffTileGlows)
        {
            if (glow != null)
            {
                Destroy(glow.gameObject);
            }
        }

        attackBuffTileGlows.Clear();
        foreach (Vector3Int tilePosition in attackBuffTiles)
        {
            GameObject glowObject = new GameObject("Attack Buff Tile Glow");
            glowObject.transform.position = tilemap.GetCellCenterWorld(tilePosition);

            AttackBuffTileGlow glow = glowObject.AddComponent<AttackBuffTileGlow>();
            glow.Initialize(tilemap.cellSize);
            attackBuffTileGlows.Add(glow);
        }
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
            int currentRound = GameManager.instance.roundIndex + 1;
            int bossIndex = currentRound / 10 - 1;

            ConfigureEnemy(spawnedBoss, bossEnemyData[bossIndex]);

            GameManager.instance.NotifyWaveSpawnComplete();
            yield break;
            //10번째마다 보스를 넣을 꺼다.
        }

        int enemyIndex = Mathf.Min(GameManager.instance.roundIndex, 29);
        for (int i = 0; i < spawnCount; i++)
        {
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

    public void SpawnMissionEnemy(int i)
    {
        GameObject spawnedEnemy = Instantiate(enemys, enemySpawnPoint.position, Quaternion.identity);
        ConfigureEnemy(spawnedEnemy, missionEnemyDatas[i]);
    }
}

public class AttackBuffTileGlow : MonoBehaviour
{
    private static Sprite glowSprite;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float phase;

    public void Initialize(Vector3 cellSize)
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetGlowSprite();
        spriteRenderer.color = new Color(1f, 0.78f, 0.12f, 0.45f);
        spriteRenderer.sortingOrder = 1;

        baseScale = new Vector3(cellSize.x * 1.15f, cellSize.y * 1.15f, 1f);
        transform.localScale = baseScale;
        phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * 3f + phase) + 1f) * 0.5f;
        transform.localScale = baseScale * Mathf.Lerp(0.9f, 1.12f, pulse);

        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.24f, 0.58f, pulse);
        spriteRenderer.color = color;
    }

    private static Sprite GetGlowSprite()
    {
        if (glowSprite != null)
        {
            return glowSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (x + 0.5f) / size * 2f - 1f;
                float normalizedY = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                float alpha = Mathf.Clamp01(1f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }

        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return glowSprite;
    }
}
