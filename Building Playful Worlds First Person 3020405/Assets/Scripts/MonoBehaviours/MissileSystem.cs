using UnityEngine;
using System.Collections;

public class MissileSystem : MonoBehaviour {

    public AudioClip clip;

    private bool once = false;

    public void OnParticleTrigger()
    {
        if (!once)
        {
            once = true;
            UITextHandler.LogText("Wait, what's going on?");
            StartCoroutine(AudioHandle());
        }
    }

    private IEnumerator AudioHandle()
    {
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.12f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.07f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.14f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.09f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.10f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.13f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.12f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.11f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.14f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        yield return new WaitForSeconds(0.08f);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.2f);
        UITextHandler.LogText("Oh no!", 3);
    }
	
}
