using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Ammo : MonoBehaviour
{
    public enum AmmoType { Pistol, Shotgun, Rifle, Health }
    public AmmoType ammoType = AmmoType.Pistol;
    public AudioClip pickupSound;
    public int ammoCount = 10;
    public UnityEvent onPickup;
    
    private void OnTriggerEnter(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            GetComponent<ParticleSystem>().Stop();
            foreach (Collider col in GetComponents<Collider>())
            {
                Destroy(col);
            }
            StartCoroutine(PickUp(player.transform));
        }
    }

    private IEnumerator PickUp(Transform player)
    {
        while (Vector3.Distance(transform.position, player.position + Vector3.down * 0.5f) > 0.5f)
        {
            transform.position = Vector3.Lerp(transform.position, player.position + Vector3.down * 0.5f, Time.deltaTime * 10);
            yield return null;
        }
        if (ammoType == AmmoType.Pistol)
        {
            player.GetComponent<PlayerController>().totalPistolAmmo += ammoCount;
        }
        if (ammoType == AmmoType.Shotgun)
        {
            player.GetComponent<PlayerController>().totalShotgunAmmo += ammoCount;
        }
        if (ammoType == AmmoType.Rifle)
        {
            player.GetComponent<PlayerController>().totalRifleAmmo += ammoCount;
        }
        if (ammoType == AmmoType.Health)
        {
            player.GetComponent<PlayerController>().health += ammoCount;
            player.GetComponent<PlayerController>().health = Mathf.Clamp(player.GetComponent<PlayerController>().health, 0, 100);
        }
        AudioSource.PlayClipAtPoint(pickupSound, player.position);
        onPickup.Invoke();
        Destroy(gameObject);
    }

}