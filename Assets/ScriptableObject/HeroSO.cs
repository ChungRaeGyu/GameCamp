using UnityEngine;

[CreateAssetMenu(fileName = "HeroSO", menuName = "Scriptable Objects/HeroSO")]
public class HeroSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string heroName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;
    [Min(0.01f)]
    [SerializeField] private float attacksPerSecond = 1f;
    [Min(0f)]
    [SerializeField] private float attackRange = 3f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [Min(0.01f)]
    [SerializeField] private float projectileSpeed = 8f;
    [Min(0.1f)]
    [SerializeField] private float projectileLifetime = 3f;

    public string HeroName => heroName;
    public Sprite Sprite => sprite;
    public RuntimeAnimatorController AnimatorController => animatorController;
    public float AttackDamage => attackDamage;
    public float AttackInterval => 1f / attacksPerSecond;
    public float AttackRange => attackRange;
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;
}
