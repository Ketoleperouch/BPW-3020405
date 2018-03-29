using UnityEngine;

[CreateAssetMenu(menuName = "Loot Collection")]
public class LootCollection : ScriptableObject
{
    public Loot[] loot;

    public void YieldLoot(Vector3 position)
    {
        for (int i = 0; i < loot.Length; i++)
        {
            if (Random.Range(0, 100) <= loot[i].rarity)
            {
                GameObject drop = Instantiate(loot[i].lootItem, position + Random.insideUnitSphere * 0.2f, Random.rotation);
                if (drop.GetComponent<Rigidbody>())
                {
                    drop.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-100f, 100f), Random.Range(0f, 100f), Random.Range(-100f, 100f)));
                }
            }
        }
    }
}