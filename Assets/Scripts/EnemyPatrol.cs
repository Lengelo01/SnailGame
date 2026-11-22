
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    private Vector3 startPosition;
    private int direction = 1;
    private Animator animator;
    private float stepDistance;
    private const float StepSpacing = 1.65f;

    void Start()
    {
        startPosition = transform.position;
        stepDistance = Mathf.Repeat(startPosition.x + startPosition.y, StepSpacing);
        animator = GetComponent<Animator>();

        animator.SetBool("MovingRight", true);
    }

    void Update()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position += Vector3.right * direction * step;
        stepDistance += Mathf.Abs(step);
        if (stepDistance >= StepSpacing)
        {
            GameSfx.Play(GameSfx.Cue.LadybugMove, transform.position);
            stepDistance %= StepSpacing;
        }

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
