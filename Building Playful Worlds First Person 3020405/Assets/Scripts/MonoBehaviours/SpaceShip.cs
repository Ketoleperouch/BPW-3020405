using UnityEngine;
using System.Collections;

public class SpaceShip : MonoBehaviour {

    public GameObject explosion;
    [Header("Timed Events")]
    public float delay;
    public ParticleSystem rockets;
    public Transform waypoint;

    private AudioSource aSource;
    private PlayerController player;
    private new MeshRenderer renderer;
    private MusicHandler music;

    private void Start()
    {
        renderer = GetComponentInChildren<MeshRenderer>();
        music = FindObjectOfType<MusicHandler>();
        player = FindObjectOfType<PlayerController>();
        aSource = GetComponent<AudioSource>();
        StartCoroutine(TimedEvent());
    }

    public void Explode()
    {
        GetComponent<ParticleSystem>().Stop();
        player.ShakeScreen(1200, 6);
        aSource.Play();
        Instantiate(explosion, transform.position, Quaternion.identity);
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = new Vector3(-5, -2, 0);
        rb.angularDrag = 0;
        rb.drag = 0;
        rb.AddTorque(new Vector3(15, 7, 4));
        player.disableMovement = false;
        player.crosshair.sprite = player.crosshairTypes[0];
        music.SwitchAudio(1, 2);
        music.lockClip = true;
        StartCoroutine(FadeRenderer());
        UITextHandler.LogText("This space station appears to have a system malfunction! All defense systems may have been activated! There's no time to " +
            "lose. Get out of this place! Go to the escape pod at the station bridge!", 0.2f);
    }

    private IEnumerator TimedEvent()
    {
        yield return new WaitForSeconds(delay);
        rockets.Play();
    }

    private IEnumerator FadeRenderer()
    {
        while (!Mathf.Approximately(renderer.material.color.a, 0))
        {
            renderer.material.color = new Color(1, 1, 1, Mathf.MoveTowards(renderer.material.color.a, 0, Time.deltaTime));
            yield return null;
        }
        WaypointSystem.SetWaypoint(waypoint);
    }
}
