using System;
using UnityEngine;
using UnityEngine.Animations;

public class Commander : MonoBehaviour, Idamagable
{
    [SerializeField] float attackspeed = 1f;
    float timer = 0;
    [SerializeField] float hp = 2000f;

    private void Update()
    {
        Attack();
        Look();
    }

    private void Look()
    {
        //몬스터 바라보게 만들기
    }

    private void Attack()
    {
        if (timer < attackspeed)
        {
            timer += Time.deltaTime;
            return;
        }
        else
        {
            //공격;;
            timer = 0;
            return;
        }
    }

    public bool TakeDamaged(float damage)
    {
        hp = Mathf.Max(hp-damage, 0);
        if (hp == 0)
        {
            GameManager.instance.GameOver();
        }
        return false;
    }
}
