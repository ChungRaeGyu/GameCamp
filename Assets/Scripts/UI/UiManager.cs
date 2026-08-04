using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    [SerializeField]private GameObject spawnButtonPrefab;
    [SerializeField] private GameObject promotionButtonPrefab;

    private GameObject spawnButton;
    private GameObject promotionButton;

    [SerializeField] private GameObject resultCanvasObject;
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private TextMeshProUGUI goldtxt;
    private void Start()
    {
        GameManager.instance.uiManager = this;
        ButtonInstantiate();
    }

    private void ButtonInstantiate()
    {
        spawnButton = Instantiate(spawnButtonPrefab, transform);
        spawnButton.SetActive(false);

        promotionButton = Instantiate(promotionButtonPrefab, transform);
        promotionButton.SetActive(false);
    }

    public void SpawnButtonActive(Vector3 pos)
    {
        spawnButton.transform.position = pos;
        spawnButton.SetActive(true);
    }
    public void PromotionActive(Vector3 pos)
    {
        promotionButton.transform.position = pos;
        promotionButton.SetActive(true);
    }
    public void ShowResult(bool isVictory)
    {
        resultText.text = isVictory ? "VICTORY" : "DEFEAT";
        resultText.color = isVictory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f);
        resultCanvasObject.SetActive(true);
    }

    public void RestartGame()
    {
        resultCanvasObject.SetActive(false);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void UpdateGold(int gold)
    {
        goldtxt.text = $"Gold : {gold}";
    }
}
