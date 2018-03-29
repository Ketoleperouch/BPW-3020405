using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour {

    public Light switchLight;
    public Color inactiveColor = Color.red;
    public Material inactive;
    public Color activeColor = Color.green;
    public Material active;
    public AudioClip switchSound;
    public bool cannotDeactivate = false;
    public UnityEvent onActivate;

    private bool state = false;
    private bool locked = false;

    public void Activate()
    {
        if (!locked)
        {
            onActivate.Invoke();
            AudioSource.PlayClipAtPoint(switchSound, transform.position);
            state = !state;
            GetComponent<MeshRenderer>().material = state ? active : inactive;
            if (switchLight)
            {
                switchLight.color = state ? activeColor : inactiveColor;
            }
            if (cannotDeactivate)
            {
                locked = true;
            }
        }
    }
	
}
