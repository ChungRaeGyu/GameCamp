using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> activeEnemies = new();

    public IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;

    private void Awake()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.enemyManager = this;
        }
    }

    public void Register(Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void Unregister(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }
}
