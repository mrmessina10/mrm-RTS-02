using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private FactionDataSO startingFactionData;

    public int CurrentGold { get; private set; }
    public int CurrentWood { get; private set; }
    public int CurrentFood { get; private set; }
    public int CurrentStone { get; private set; }

    public event Action<int> OnGoldChanged;
    public event Action<int> OnWoodChanged;
    public event Action<int> OnFoodChanged;
    public event Action<int> OnStoneChanged;

    private void Start()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        AddGold(startingFactionData.startingGold);
        AddWood(startingFactionData.startingWood);
        AddFood(startingFactionData.startingFood);
        AddStone(startingFactionData.startingStone);
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public void AddWood(int amount)
    {
        CurrentWood += amount;
        OnGoldChanged?.Invoke(CurrentWood);
    }
    public void AddFood(int amount)
    {
        CurrentFood += amount;
        OnGoldChanged?.Invoke(CurrentFood);
    }
    public void AddStone(int amount)
    {
        CurrentStone += amount;
        OnGoldChanged?.Invoke(CurrentStone);
    }
}
