using UnityEngine;

public class Destructable : MonoBehaviour
{
    public float health = 4;
    public GameObject hitParticles;
    public GameObject destructionParticles;
    public AudioClip[] damageSounds;
    public AudioClip destructionSound;
    public LootCollection[] loot;
    public bool shakeScreenOnDestroy = false;

    private bool droppedLoot = false;   //Prevents multiple loot drops to be dropped (e.g. when enemy has been annihilated with the shotgun)

    public void Damage(Vector3 point, float damage)
    {
        if (GetComponent<Turret>())
        {
            GetComponent<Turret>().target = FindObjectOfType<PlayerController>().transform;
            GetComponent<Turret>().loseTargetTimer = Time.time + GetComponent<Turret>().detectionTime;
        }
        if (GetComponent<PatrolTurret>())
        {
            GetComponent<PatrolTurret>().token.currentTarget = FindObjectOfType<PlayerController>().transform;
        }
        AudioSource.PlayClipAtPoint(damageSounds[Random.Range(0, damageSounds.Length)], point);
        Instantiate(hitParticles, point, Quaternion.identity);
        health -= damage;
        if (health <= 0)
        {
            Destroy();
        }
    }

    public void Destroy()
    {
        AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        Instantiate(destructionParticles, transform.position, Quaternion.identity);
        if (shakeScreenOnDestroy)
        {
            FindObjectOfType<PlayerController>().ShakeScreen(5, Mathf.Abs(5 - (Vector3.Distance(transform.position, FindObjectOfType<PlayerController>().transform.position))));
        }
        if (loot != null && loot.Length > 0 && !droppedLoot)
        {
            for (int i = 0; i < loot.Length; i++)
            {
                loot[i].YieldLoot(transform.position);
            }
            droppedLoot = true;
        }
        Destroy(gameObject);
    }
	
}
