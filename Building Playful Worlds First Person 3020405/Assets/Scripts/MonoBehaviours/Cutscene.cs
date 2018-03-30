using System.Collections;
using UnityEngine;

public class Cutscene : MonoBehaviour {

    [Header("Scene 1")]
    public Transform cam;
    public Transform cam2;
    public Transform[] camWaypoints;
    public Transform playerLookTarget;
    public GameObject laserBundle;
    public AudioClip laserShutdownClip;
    public AudioClip tension1;
    public AudioClip tension2;
    public AudioSource escapePodSource;
    public Light engineLight;

    private MusicHandler music;
    private PlayerController player;
    private float catchTimer;   //Used in Scene3 when player Rotation aligning doesn't work correctly.

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
        catchTimer = Time.time + 5;
        if (music.currentPlayingSource == 2)
        {
            music.audioSources[0].clip = tension2;
            music.SwitchAudio(0, 4);
        }
        else
        {
            music.audioSources[1].clip = tension2;
            music.SwitchAudio(1, 4);
        }
        Debug.Log(catchTimer + ", " + Time.time);
        player.disableMovement = true;
        player.enabled = false;
        UITextHandler.LogText("I'm activating the pod's engines. We'll be out of here in no time.", 1f);
        escapePodSource.Play();
        while (player.transform.rotation != playerLookTarget.rotation)
        {
            player.transform.rotation = Quaternion.Slerp(player.transform.localRotation, playerLookTarget.localRotation, Time.deltaTime);
            if (engineLight.intensity < 4)
                engineLight.intensity += 0.03f;
            if (Time.time > catchTimer)
            {
                player.transform.rotation = playerLookTarget.rotation;
            }
            yield return null;
        }
        player.ShakeScreen(10, 0.5f);
        yield return new WaitForSeconds(1);
        player.ShakeScreen(10, 1f);
        yield return StartCoroutine(player.FadeBlackScreen(1));
        yield return new WaitForSeconds(5);
        while (music.audioSources[music.currentPlayingSource - 1].volume > 0)
        {
            Debug.Log(music.audioSources[music.currentPlayingSource - 1].volume);
            music.audioSources[music.currentPlayingSource - 1].volume = Mathf.MoveTowards(music.audioSources[music.currentPlayingSource - 1].volume, 0, Time.deltaTime / 5);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndingScreen");
    }

}
