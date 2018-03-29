using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TargetMover : MonoBehaviour {

    public float speed;
    public Transform target;
    public UnityEvent onTargetReached;

    private IEnumerator Start()
    {
        while (Vector3.Distance(transform.position, target.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * speed);
            yield return null;
        }
        onTargetReached.Invoke();
    }
}
