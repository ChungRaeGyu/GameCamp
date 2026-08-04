using UnityEngine;

public class SpawnButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SpawnHeroButton()
    {
        if (GameManager.instance.IsGameEnded)
        {
            return;
        }

        GameManager.instance.moneyManager.AddGold(-40);
        this.gameObject.SetActive(false);
        GameManager.instance.spawnManager.SpawnHeroButton();
    }
}
