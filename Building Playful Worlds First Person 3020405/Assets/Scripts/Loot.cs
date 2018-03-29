using UnityEngine;

[System.Serializable]
public class Loot
{
    public GameObject lootItem;
    [Range(0, 100)]
    public float rarity = 100.0f;

}