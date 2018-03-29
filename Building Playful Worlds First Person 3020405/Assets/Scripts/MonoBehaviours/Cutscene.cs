using System.Collections;
using UnityEngine;

public class Cutscene : MonoBehaviour {

    [Header("Scene 1")]
    public Transform cam;
    public Transform cam2;
    public Transform[] camWaypoints;
    public GameObject laserBundle;
    public AudioClip laserShutdownClip;
    public AudioClip tension1;

    private MusicHandler music;
    private PlayerController player;

    private void Start()
    {
        music = FindObjectOfType<MusicHandler>();
        player = FindObjectOfType<PlayerController>();
    }

    public void ActivateScene(int scene)
    {
        IEnumerator routine = scene == 0 ? Scene1() : scene == 1 ? Scene2() : Scene3();
        StartCoroutine(routine);
    }

    private IEnumerator Scene1()
    {
        music.lockClip = false;
        music.forceSwitch = true;
        player.enabled = false;
        yield return StartCoroutine(player.FadeBlackScreen(1));
        cam.gameObject.SetActive(true);
        StartCoroutine(player.FadeBlackScreen(0));
        yield return new WaitForSeconds(1);
        while (Vector3.Distance(cam.position, camWaypoints[0].position) > 0.05f)
        {
            cam.position = Vector3.MoveTowards(cam.position, camWaypoints[0].position, Time.deltaTime * 4);
            if (cam.rotation != camWaypoints[0].rotation)
                cam.rotation = Quaternion.RotateTowards(cam.rotation, camWaypoints[0].rotation, Time.deltaTime * 12);
            yield return null;
        }
        while (Vector3.Distance(cam.position, camWaypoints[1].position) > 0.05f)
        {
            cam.position = Vector3.MoveTowards(cam.position, camWaypoints[1].position, Time.deltaTime * 4);
            if (cam.rotation != camWaypoints[1].rotation)
                cam.rotation = Quaternion.RotateTowards(cam.rotation, camWaypoints[1].rotation, Time.deltaTime * 16);
            yield return null;
        }
        while (Vector3.Distance(cam.position, camWaypoints[2].position) > 0.05f)
        {
            cam.position = Vector3.MoveTowards(cam.position, camWaypoints[2].position, Time.deltaTime * 4);
            if (cam.rotation != camWaypoints[2].rotation)
                cam.rotation = Quaternion.Slerp(cam.rotation, camWaypoints[2].rotation, Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        UITextHandler.LogText("The switch we're looking for is located at the other side of this room. Be careful of the floating turrets here. " +
            "If they find you, they won't stop attacking until you're dead.");
        yield return StartCoroutine(player.FadeBlackScreen(1));
        cam.gameObject.SetActive(false);
        yield return StartCoroutine(player.FadeBlackScreen(0));
        player.enabled = true;
        music.audioSources[1].clip = tension1;
    }

    private IEnumerator Scene2()
    {
        player.enabled = false;
        yield return StartCoroutine(player.FadeBlackScreen(1));
        cam2.gameObject.SetActive(true);
        StartCoroutine(player.FadeBlackScreen(0));
        yield return new WaitForSeconds(1);
        laserBundle.SetActive(false);
        AudioSource.PlayClipAtPoint(laserShutdownClip, player.transform.position);
        yield return new WaitForSeconds(2);
        UITextHandler.LogText("There we go. The lasers have been shut off. The way to the escape pod is clear!");
        yield return StartCoroutine(player.FadeBlackScreen(1));
        cam2.gameObject.SetActive(false);
        yield return StartCoroutine(player.FadeBlackScreen(0));
        player.enabled = true;
    }

    private IEnumerator Scene3()
    {
        yield break;
    }

}
