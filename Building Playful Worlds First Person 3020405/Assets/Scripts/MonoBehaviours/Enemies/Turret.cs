using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyToken))]
public class Turret : MonoBehaviour
{
    public Transform turretHead;
    public Transform turretEyes;
    public Transform turretBarrel;
    public Transform[] barrels;
    public float idleTurnSpeed = 20f;
    public float detectionRange = 20.0f;
    public float detectionRadius = 1f;
    public float fireRate = 0.1f;
    public float detectionTime = 5f;
    public int turretPower = 1;
    public string targetTag = "Player";
    public AudioClip[] shotSounds;
    public AudioSource audioSource;

    public Transform target { get; set; }
    public float loseTargetTimer { get; set; }

    private float nextShot;
    private float turningSpeed;
    private int barrelReady = 0;
    private EnemyToken token;
    private MusicHandler music;

    private void Start()
    {
        token = GetComponent<EnemyToken>();
        music = FindObjectOfType<MusicHandler>();

        //Set detection time if the turret already has a target.
        if (token.currentTarget)
        {
            loseTargetTimer = Time.time + detectionTime;
        }
    }

    private void Update()
    {
        //Rotate turret if no target
        if (!target)
        {
            turretHead.Rotate(new Vector3(0, idleTurnSpeed, 0) * Time.deltaTime);
            target = AcquireTarget();
        }
        else
        {
            if (music.currentPlayingSource != 2 && Time.time < loseTargetTimer)
            {
                music.SwitchAudio(1, 1);
            }
            //Look at target
            Vector3 targetPos = new Vector3(target.position.x, turretHead.position.y, target.position.z);

            turretHead.LookAt(Vector3.Lerp(turretHead.position + turretHead.forward, targetPos, Time.deltaTime * 3));
            Fire();
            if (Time.time >= loseTargetTimer)
            {
                target = null;
                if (token.player && !token.player.InDanger())
                {
                    music.SwitchAudio(0, 1);
                }
            }
        }
        turningSpeed--;
        turningSpeed = Mathf.Clamp(turningSpeed, 0, 100);
    }

    private void Fire()
    {
        //Turn barrel
        turretBarrel.Rotate(Vector3.right, turningSpeed * 20 * Time.deltaTime);
        //Raycast, look for target.
        RaycastHit hit;
        if (Time.time > nextShot && CanHit())
        {
            turningSpeed += 100;
            loseTargetTimer = Time.time + detectionTime;
            nextShot = Time.time + fireRate;
            barrels[barrelReady].GetComponent<ParticleSystem>().Play();
            audioSource.clip = shotSounds[Random.Range(0, shotSounds.Length)];
            audioSource.Play();
            if (Physics.Raycast(barrels[barrelReady].position, barrels[barrelReady].forward, out hit, detectionRange * 2))
            {
                if (hit.collider.CompareTag(targetTag))
                {
                    hit.collider.GetComponent<PlayerController>().TakeDamage(turretPower, hit.point);
                }
            }
            barrelReady = barrelReady++ % barrels.Length;
        }
        
    }

    private bool CanHit()
    {
        RaycastHit hit;
        if (Physics.SphereCast(turretEyes.position, detectionRadius * 2, turretEyes.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(targetTag) && !hit.collider.GetComponent<PlayerController>().dead)
            {
                return true;
            }
        }
        return false;
    }

    private Transform AcquireTarget()
    {
        RaycastHit hit;
        if (Physics.SphereCast(turretEyes.position, detectionRadius, turretEyes.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                token.currentTarget = hit.transform;
                return hit.transform;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        if (FindObjectOfType<PlayerController>() && FindObjectOfType<PlayerController>().InDanger() == false)
        {
            music.SwitchAudio(0, 1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(turretEyes.position, detectionRadius);
        Gizmos.DrawLine(turretEyes.position, turretEyes.position + turretEyes.forward * detectionRange);
    }

}
