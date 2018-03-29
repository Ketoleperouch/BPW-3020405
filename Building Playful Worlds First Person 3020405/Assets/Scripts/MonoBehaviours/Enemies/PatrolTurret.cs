using UnityEngine.AI;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyToken))]
public class PatrolTurret : MonoBehaviour {

    public Transform[] patrolWaypoints;
    public enum State { Patrol, Detect, Attack}
    public State currentState;
    public Transform eyes;
    public Transform turretHead;
    public Transform barrel;
    public float idleTurnSpeed = 20f;
    public float detectionRange = 20.0f;
    public float detectionRadius = 1f;
    public float fireRate = 0.1f;
    public float detectionTime = 5f;
    public int turretPower = 1;
    public string targetTag = "Player";
    public AudioClip[] shotSounds;
    public AudioSource audioSource;

    [HideInInspector] public EnemyToken token;

    private int state;
    private int nextWaypoint;
    private NavMeshAgent agent;
    private MusicHandler music;
    private float nextShot;
    private float originalRadius;

    private void Start()
    {
        music = FindObjectOfType<MusicHandler>();
        agent = GetComponent<NavMeshAgent>();
        token = GetComponent<EnemyToken>();
        originalRadius = detectionRadius;
    }

    private void Update()
    {
        if (!agent)
        {
            return;
        }
        state = (int)currentState;

        switch (state)
        {
            case 0:
                Patrol();
                if (!token.currentTarget)
                {
                    turretHead.Rotate(new Vector3(0, idleTurnSpeed, 0) * Time.deltaTime);
                    token.currentTarget = AcquireTarget();
                }
                else
                {
                    TransitionToState(State.Detect);
                }
                break;
            case 1:
                Detect();
                break;
            case 2:
                Attack();
                break;
            default:
                TransitionToState(State.Patrol);
                break;
        }
    }

    private void Patrol()
    {
        agent.destination = patrolWaypoints[nextWaypoint].position;
        agent.isStopped = false;
        detectionRadius = originalRadius;
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            nextWaypoint = (nextWaypoint + 1) % patrolWaypoints.Length;
        }
    }

    private void Detect()
    {
        //Check for targets, attack if target is in fire range. Detect acts as a double check.
        if (CanHit())
        {
            if (music.currentPlayingSource != 2)
            {
                music.SwitchAudio(1, 1);
            }
            detectionRadius = originalRadius / 3;
            TransitionToState(State.Attack);
        }
        else
        {
            TransitionToState(State.Patrol);
        }
    }

    private void Attack()
    {
        //Look at target, target is a little lower due to inconvenient pivot offset
        turretHead.LookAt(Vector3.Lerp(turretHead.position + turretHead.forward, token.currentTarget.position + Vector3.down / 2, Time.deltaTime));
        RaycastHit hit;
            if (Time.time > nextShot && CanHit())
            {
                nextShot = Time.time + fireRate;
                barrel.GetComponent<ParticleSystem>().Play();
                audioSource.clip = shotSounds[Random.Range(0, shotSounds.Length)];
                audioSource.Play();
                if (Physics.Raycast(barrel.position, barrel.forward, out hit, detectionRange * 2))
                {
                    if (hit.collider.CompareTag(targetTag))
                    {
                        hit.collider.GetComponent<PlayerController>().TakeDamage(turretPower, hit.point);
                    }
                }
            }
        //Set agent path to Player
        agent.SetDestination(token.currentTarget.position);
        }

    private Transform AcquireTarget()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.SphereCast(eyes.position, detectionRadius, eyes.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                return hit.transform;
            }
        }
        return null;
    }

    private bool CanHit()
    {
        RaycastHit hit;
        if (Physics.SphereCast(eyes.position, detectionRadius * 2, eyes.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(targetTag) && !hit.collider.GetComponent<PlayerController>().dead)
            {
                return true;
            }
        }
        return false;
    }

    private void TransitionToState(State toState)
    {
        if ((int)toState != state)
        {
            currentState = toState;
        }
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
        Gizmos.DrawWireSphere(eyes.position, detectionRadius);
        Gizmos.DrawLine(eyes.position, eyes.position + eyes.forward * detectionRange);
    }
}
