using UnityEngine;
public class Hero : MonoBehaviour
{
    [SerializeField] private HeroSO heroData;
    [SerializeField] private Transform projectileSpawnPoint;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float attackTimer;

    private Enemy target;


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

        if (target != null)
        {
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

        Fire(target);
        attackTimer = heroData.AttackInterval;
    }

    public void Configure(HeroSO data)
    {
        heroData = data;
        ApplyHeroData();
    }

    private void ApplyHeroData()
    {
        if (heroData == null)
        {
            return;
        }

        animator.runtimeAnimatorController = heroData.AnimatorController;
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

        projectile.Initialize(target, heroData.AttackDamage, heroData.ProjectileSpeed, heroData.ProjectileLifetime);
    }
}
