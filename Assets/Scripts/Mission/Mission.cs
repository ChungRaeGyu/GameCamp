using UnityEngine;

public class Mission : MonoBehaviour
{
    [SerializeField] private GameObject missionPanel;
    public void OpenPanel()
    {
        missionPanel.SetActive(true);
    }
    public void ClosePanel()
    {
        missionPanel.SetActive(false);
    }

    public void MissionSpawnButton(int i)
    {
        GameManager.instance.spawnManager.SpawnMissionEnemy(i);
    }

}
