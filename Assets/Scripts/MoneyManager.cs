using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private int gold = 0;
    [SerializeField] private int cristal = 0;
    private void Start()
    {
        GameManager.instance.moneyManager = this;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"골드 추가: {amount}, 현재 골드: {gold}");
    }
    public int GetGold() { return gold; }

    public void AddCristal(int amount)
    {
        cristal += amount;
    }
    public int GetCristal() { return cristal; }
}
