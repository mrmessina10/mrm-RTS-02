using UnityEngine;

[CreateAssetMenu(fileName = "New Faction Data", menuName = "ScriptableObjects/FactionDataSO")]
public class FactionDataSO : ScriptableObject
{
    public string factionName;
    public Color factionColor;
    public int startingGold;
    public int startingWood;
    public int startingFood;
    public int startingStone;
}
