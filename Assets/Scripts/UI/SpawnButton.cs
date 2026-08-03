using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SpawnHeroButton()
    {
        this.gameObject.SetActive(false);
        GameManager.instance.spawnManager.SpawnHeroButton();
    }
}
