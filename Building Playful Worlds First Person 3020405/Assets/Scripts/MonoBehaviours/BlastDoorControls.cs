using UnityEngine;

public class BlastDoorControls : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        
        anim = GetComponent<Animator>();
    }

    public void Activate(bool state)
    {
        anim.SetBool("Open", state);
    }
}