using UnityEngine;

public class DoorAutoActivate : MonoBehaviour {

    public AudioClip openSound;
    public AudioClip closeSound;
    public bool locked = false;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void SetLock(bool state)
    {
        locked = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!locked)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
            anim.SetBool("Open", true);
        }
        else
        {
            UITextHandler.LogText("This door is locked.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!locked)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
            anim.SetBool("Open", false);
        }
    }
}
