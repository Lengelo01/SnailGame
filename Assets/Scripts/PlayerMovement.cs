
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    public bool IsInShell { get; private set; }

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        IsInShell = true;
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            movement = Vector2.zero;
            IsInShell = true;
            animator.SetBool("IsMoving", false);
            return;
        }

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
            moveX = -1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            moveX = 1f;

        if (Keyboard.current.upArrowKey.isPressed)
            moveY = 1f;

        if (Keyboard.current.downArrowKey.isPressed)
            moveY = -1f;

        movement = new Vector2(moveX, moveY).normalized;

        IsInShell = movement == Vector2.zero;

        animator.SetBool("IsMoving", !IsInShell);

        if (movement != Vector2.zero)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}
