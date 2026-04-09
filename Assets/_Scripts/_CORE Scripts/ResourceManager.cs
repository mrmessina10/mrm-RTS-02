using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>();
    public event Action<ResourceType, int> OnResourceChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            inventory[type] = 0;
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        inventory[type] += amount;
        OnResourceChanged?.Invoke(type, inventory[type]);
        Debug.Log($"[ResourceManager] +{amount} {type}. Total: {inventory[type]}");
    }
}
