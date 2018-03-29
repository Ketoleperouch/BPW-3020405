using UnityEngine;

public class EnemyToken : MonoBehaviour
{
    public Transform currentTarget;

    [HideInInspector] public PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }
}