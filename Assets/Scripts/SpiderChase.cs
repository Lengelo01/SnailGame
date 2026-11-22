
using UnityEngine;

public class SpiderChase : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float returnSpeed = 3f;

    private Transform player;
    private Rigidbody2D rb;

    private Vector2 startPosition;
    private Vector2 lastBodyPosition;
    private float scuttleDistance;
    private const float ScuttleSpacing = 0.75f;

    private bool isChasing = false;
    private bool isReturning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        startPosition = rb.position;
        lastBodyPosition = rb.position;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    void FixedUpdate()
    {
        scuttleDistance += Vector2.Distance(lastBodyPosition, rb.position);
        lastBodyPosition = rb.position;
        if ((isChasing || isReturning) && scuttleDistance >= ScuttleSpacing)
        {
            GameSfx.Play(GameSfx.Cue.SpiderMove, rb.position);
            scuttleDistance %= ScuttleSpacing;
        }

        if (isChasing && player != null)
        {
            MoveTowards((Vector2)player.position, moveSpeed);
        }
        else if (isReturning)
        {
            MoveTowards(startPosition, returnSpeed);

            if (Vector2.Distance(rb.position, startPosition) < 0.05f)
            {
                rb.MovePosition(startPosition);
                isReturning = false;
            }
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    public void StartChasing()
    {
        isChasing = true;
        isReturning = false;
    }

    public void StopChasing()
    {
        isChasing = false;
        isReturning = true;
    }
}
