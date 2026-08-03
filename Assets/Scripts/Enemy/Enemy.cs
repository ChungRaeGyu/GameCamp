using System;
using UnityEngine;
public interface Idamagable
{
    public bool TakeDamaged(float damage);
}
public class Enemy : MonoBehaviour, Idamagable
{
    // Update is called once per frame
    int index = 0;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 0.5f;
    private float attackTimer;
    private Commander commander;

    private void Start()
    {
        commander = FindAnyObjectByType<Commander>();
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

            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.05f)
            {
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

        if (commander == null)
        {
            commander = FindAnyObjectByType<Commander>();
        }

        if (commander != null)
        {
            commander.TakeDamaged(attackDamage);
        }
    }
}
