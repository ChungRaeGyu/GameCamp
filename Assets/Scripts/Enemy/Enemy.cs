using System;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
public interface Idamagable
{
    public bool TakeDamaged(float damage);
}
public class Enemy : MonoBehaviour, Idamagable
{
    // Update is called once per frame
    int index = 0;
    [SerializeField] private float speed = 1.0f;
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
        if (index >= GameManager.instance.waypoints.Length) return;
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
