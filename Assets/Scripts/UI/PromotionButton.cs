using UnityEngine;

public class PromotionButton : MonoBehaviour
{
    public void Promotion()
    {
        if (GameManager.instance.IsGameEnded)
        {
            return;
        }

        gameObject.SetActive(false);
        GameManager.instance.spawnManager.PromotionButton();
    }
}
