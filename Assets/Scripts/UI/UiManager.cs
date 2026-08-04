using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField]private GameObject spawnButtonPrefab;
    [SerializeField] private GameObject promotionButtonPrefab;

    private GameObject spawnButton;
    private GameObject promotionButton;

    private GameObject resultCanvasObject;
    private GameObject resultPanel;
    private TextMeshProUGUI resultText;
    private void Start()
    {
        GameManager.instance.uiManager = this;
        ButtonInstantiate();

        CreateResultPanel();
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
        promotionButton.SetActive(true);
    }
    public void PromotionActive(Vector3 pos)
    {
        promotionButton.transform.position = pos;
        spawnButton.SetActive(true);
    }
    public void ShowResult(bool isVictory)
    {
        resultText.text = isVictory ? "VICTORY" : "DEFEAT";
        resultText.color = isVictory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f);
        resultCanvasObject.SetActive(true);
    }

    private void CreateResultPanel()
    {
        resultCanvasObject = new GameObject("ResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas resultCanvas = resultCanvasObject.GetComponent<Canvas>();
        resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        resultCanvas.sortingOrder = 100;

        resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
        resultPanel.transform.SetParent(resultCanvasObject.transform, false);
        Image panelImage = resultPanel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f);

        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(resultPanel.transform, false);
        resultText = textObject.GetComponent<TextMeshProUGUI>();
        resultText.font = TMP_Settings.defaultFontAsset;
        resultText.fontSize = 72f;
        resultText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = resultText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800f, 180f);
        textRect.anchoredPosition = Vector2.zero;

        resultCanvasObject.SetActive(false);
    }
}
