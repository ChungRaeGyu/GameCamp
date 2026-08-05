using UnityEngine;
public enum HeroType
{
    Normal,
    Rare,
    Myth,
    Legendary,
    Random
}
public class Hero : MonoBehaviour
{
    [SerializeField] private HeroSO heroData;
    [SerializeField] private Transform projectileSpawnPoint;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float attackTimer;
    private float tileAttackMultiplier = 1f;

    private Enemy target;

    public Vector3Int tilePos;

    public HeroSO Data => heroData;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (heroData == null || heroData.ProjectilePrefab == null)
        {
            return;
        }

        if (attackTimer > 0f)
        {
            if (target != null)
            {
                FaceTarget(target);
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
            {
                return;
            }
        }

        target = FindTarget();
        if (target == null)
        {
            return;
        }

        FaceTarget(target);
        Fire(target);
        attackTimer = heroData.AttackInterval;
    }

    public void Configure(HeroSO data)
    {
        heroData = data;
        ApplyHeroData();
    }

    public void SetTileAttackMultiplier(float multiplier)
    {
        tileAttackMultiplier = Mathf.Max(0f, multiplier);
    }

    private void ApplyHeroData()
    {
        if (heroData == null)
        {
            return;
        }

        if (spriteRenderer != null && heroData.Sprite != null)
        {
            spriteRenderer.sprite = heroData.Sprite;
        }

        if (animator != null)
        {
            animator.runtimeAnimatorController = heroData.AnimatorController;
        }
    }

    private Enemy FindTarget()
    {
        if (GameManager.instance == null || GameManager.instance.enemyManager == null)
        {
            return null;
        }

        Enemy leadingEnemy = null;
        float highestProgress = float.MinValue;
        float attackRangeSqr = heroData.AttackRange * heroData.AttackRange;

        foreach (Enemy enemy in GameManager.instance.enemyManager.ActiveEnemies)
        {
            if (enemy == null ||
                ((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude > attackRangeSqr ||
                enemy.Progress <= highestProgress)
            {
                continue;
            }

            leadingEnemy = enemy;
            highestProgress = enemy.Progress;
        }

        return leadingEnemy;
    }

    private void Fire(Enemy target)
    {
        Transform spawnPoint = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        GameObject projectileObject = Instantiate(heroData.ProjectilePrefab, spawnPoint.position, Quaternion.identity);
        HeroProjectile projectile = projectileObject.GetComponent<HeroProjectile>();

        if (projectile == null)
        {
            Debug.LogWarning($"{heroData.HeroName}의 탄환 프리팹에 HeroProjectile 컴포넌트가 없습니다.");
            Destroy(projectileObject);
            return;
        }
        float enhanceAmount = GameManager.instance.enhance.enhanceAmount[(int)heroData.HeroType];
        projectile.Initialize(target, heroData.AttackDamage * enhanceAmount * tileAttackMultiplier, heroData.ProjectileSpeed, heroData.ProjectileLifetime);
    }

    private void FaceTarget(Enemy attackTarget)
    {
        if (spriteRenderer == null || attackTarget == null)
        {
            return;
        }

        float horizontalDistance = attackTarget.transform.position.x - transform.position.x;
        if (!Mathf.Approximately(horizontalDistance, 0f))
        {
            spriteRenderer.flipX = horizontalDistance < 0f;
        }
    }
}
