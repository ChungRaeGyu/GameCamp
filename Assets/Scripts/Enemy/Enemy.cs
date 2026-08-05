using UnityEngine;
public interface Idamagable
{
    public bool TakeDamaged(float damage);
}
public class Enemy : MonoBehaviour, Idamagable
{
    // Update is called once per frame
    int index = 0;
    [SerializeField] private EnemySO enemyData;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 0.5f;
    private float attackTimer;
    private Commander commander;
    private Vector2 segmentStartPosition;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private int currentWalkState;

    public float Progress
    {
        get
        {
            if (index >= GameManager.instance.waypoints.Length)
            {
                return GameManager.instance.waypoints.Length;
            }

            Vector2 nextWaypointPosition = GameManager.instance.waypoints[index].position;
            float segmentLength = Vector2.Distance(segmentStartPosition, nextWaypointPosition);
            if (segmentLength <= Mathf.Epsilon)
            {
                return index;
            }

            float segmentProgress = 1f - Vector2.Distance(transform.position, nextWaypointPosition) / segmentLength;
            return index + Mathf.Clamp01(segmentProgress);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyEnemyData();
        commander = FindAnyObjectByType<Commander>();
        segmentStartPosition = transform.position;
        GameManager.instance.enemyManager?.Register(this);
    }

    public void Configure(EnemySO data)
    {
        enemyData = data;
        ApplyEnemyData();
    }

    private void ApplyEnemyData()
    {
        if (enemyData == null)
        {
            return;
        }

        speed = enemyData.MoveSpeed;
        hp = enemyData.Hp;
        attackDamage = enemyData.AttackDamage;
        attackInterval = enemyData.AttackInterval;

        if (spriteRenderer != null && enemyData.Sprite != null)
        {
            spriteRenderer.sprite = enemyData.Sprite;
        }

        if (enemyData.AnimatorController != null)
        { 
            animator.runtimeAnimatorController = enemyData.AnimatorController;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.enemyManager?.Unregister(this);
        }
    }
    float hp = 100;  //나중에 라운드당 Hp를 추가로 줄것이다.

    public bool TakeDamaged(float damage)
    {
        hp = Mathf.Max(hp - damage, 0);

        if(hp == 0)
        {
            Dead();
            return true;
        }
        return false;
    }

    private void Dead()
    {
        //나중에 적 SO를 만들어서 골드까지 넣자
        GameManager.instance.moneyManager.AddGold(enemyData.Gold);
        Destroy(gameObject);
    }

    void Update()
    {
        if (index >= GameManager.instance.waypoints.Length)
        {
            Attack();
        }
        else
        {
            Transform target = GameManager.instance.waypoints[index];
            UpdateMoveDirection(target.position - transform.position);

            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.05f)
            {
                segmentStartPosition = target.position;
                index++;
            }
        }
    }

    private void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        commander.TakeDamaged(attackDamage);
    }

    private void UpdateMoveDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.01f)
        {
            return;
        }

        bool movingLeft = direction.x < 0f;
        int walkState = Animator.StringToHash(movingLeft ? "Left_Walk-Walk" : "Right_Walk-Walk");

        if (animator != null && animator.HasState(0, walkState))
        {
            if (currentWalkState != walkState)
            {
                animator.Play(walkState);
                currentWalkState = walkState;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = movingLeft;
        }
    }
}
