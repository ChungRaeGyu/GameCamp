using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private float heroDestroyInterval = 5f;
    private float heroDestroyTimer;

    private void Update()
    {
        if (GameManager.instance == null || GameManager.instance.IsGameEnded)
        {
            return;
        }

        heroDestroyTimer += Time.deltaTime;
        if (heroDestroyTimer < heroDestroyInterval)
        {
            return;
        }

        heroDestroyTimer = 0f;
        if (GameManager.instance.spawnManager != null)
        {
            GameManager.instance.spawnManager.DestroyRandomHero();
        }
    }
}
