
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    private Vector3 startPosition;
    private int direction = 1;
    private Animator animator;

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();

        animator.SetBool("MovingRight", true);
    }

    void Update()
    {
        transform.position +=
            Vector3.right * direction * moveSpeed * Time.deltaTime;

        float distance =
            transform.position.x - startPosition.x;

        if (distance >= patrolDistance)
        {
            direction = -1;
        }
        else if (distance <= -patrolDistance)
        {
            direction = 1;
        }

        animator.SetBool("MovingRight", direction == 1);
    }
}
