using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private RuntimeAnimatorController animatorController;


    [Header("Combat")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float attackDamage = 10f;
    [Min(0.01f)]
    [SerializeField] private float attacksPerSecond = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private int gold = 10;

    public string EnemyName => enemyName;
    public Sprite Sprite => sprite;
    public RuntimeAnimatorController AnimatorController => animatorController;
    public float AttackDamage => attackDamage;
    public float AttackInterval => 1f / attacksPerSecond;
    public float MoveSpeed => speed;
    public float Hp => hp;
    public int Gold => gold;
    public void ConfigureStats(string name, float health, float damage, float attackSpeed, float moveSpeed)
    {
        enemyName = name;
        hp = Mathf.Max(health, 1f);
        attackDamage = damage;
        attacksPerSecond = Mathf.Max(attackSpeed, 0.01f);
        speed = moveSpeed;
    }

}
