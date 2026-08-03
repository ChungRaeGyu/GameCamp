using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField]private GameObject spawnButtonPrefab;
    private GameObject spawnButton;
    private void Start()
    {
        GameManager.instance.uiManager = this;
        ButtonInstantiate();
    }

    private void ButtonInstantiate()
    {
        spawnButton = Instantiate(spawnButtonPrefab, transform);
        spawnButton.transform.SetParent(transform);
        spawnButton.SetActive(false);
    }

    public void SpawnButtonActive(Vector3 pos)
    {
        spawnButton.transform.position = pos;
        spawnButton.SetActive(true);
    }
}
