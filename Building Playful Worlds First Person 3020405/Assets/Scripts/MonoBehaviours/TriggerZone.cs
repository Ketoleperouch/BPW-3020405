using UnityEngine.Events;
using UnityEngine;

public class TriggerZone : MonoBehaviour {

    public UnityEvent onTrigger;
    public enum TriggerType {Enter, Exit}
    public TriggerType triggerType;
    public bool triggerOnce = true;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() && triggerType == 0)
        {
            if (triggerOnce && triggered)
            {
                return;
            }
            triggered = true;
            onTrigger.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() && (int)triggerType == 1)
        {
            if (triggerOnce && triggered)
            {
                return;
            }
            triggered = true;
            onTrigger.Invoke();
        }
    }
}
