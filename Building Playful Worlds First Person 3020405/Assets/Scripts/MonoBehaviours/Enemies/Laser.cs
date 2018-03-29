using UnityEngine;

public class Laser : MonoBehaviour {

    public int damage;

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if (!other.GetComponent<PlayerController>().dead)
                other.GetComponent<PlayerController>().TakeDamage(damage, transform.position);
        }
    }
}
