using UnityEngine;

public class HeroProjectile : MonoBehaviour
{
    private Enemy target;
    private float damage;
    private float speed;
    private float remainingLifetime;

    public void Initialize(Enemy targetEnemy, float attackDamage, float projectileSpeed, float lifetime)
    {
        target = targetEnemy;
        damage = attackDamage;
        speed = projectileSpeed;
        remainingLifetime = lifetime;
    }

    private void Update()
    {
        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f || target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target.transform.position) > 0.05f)
        {
            return;
        }

        target.TakeDamaged(damage);
        Destroy(gameObject);
    }
}
