using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Update is called once per frame
    int index = 0;
    [SerializeField] private float speed = 1.0f;
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
