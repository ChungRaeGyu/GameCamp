using UnityEngine;

public class Enhance : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public float[] enhanceAmount = new float[]{1f,1f,1f,1f};

    public void Start()
    {
        GameManager.instance.enhance = this;
    }
    public void OpenPanel()
    {
        panel.SetActive(true);
    }
    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void EnhanceBtn(int type)
    {
        //HeroType을 사용해서 증가 수치를 다르게 할 수도 있음
        enhanceAmount[type] += 0.2f;
    }
}
