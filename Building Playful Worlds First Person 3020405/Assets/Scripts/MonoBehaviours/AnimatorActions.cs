using UnityEngine;

public class AnimatorActions : MonoBehaviour {

    private PlayerController controller;

    private void Start()
    {
        controller = FindObjectOfType<PlayerController>();
    }

    public void PlayReloadSound()
    {
        //Animator Event
        controller.aSource.clip = controller.reloadSound;
        controller.aSource.Play();
    }
}
