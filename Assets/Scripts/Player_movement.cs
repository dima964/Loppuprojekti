
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Read movement using the new Input System
        if (Keyboard.current != null)
        {
            moveInput = Vector2.zero;

            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
                moveInput.y += 1f;

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
                moveInput.y -= 1f;

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
                moveInput.x += 1f;

            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
                moveInput.x -= 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(
            moveInput.x, 0f, moveInput.y
        ).normalized;

        // Preserve vertical velocity for gravity and jumping
        Vector3 velocity = rb.linearVelocity;
        velocity.x = movement.x * moveSpeed;
        velocity.z = movement.z * moveSpeed;
        rb.linearVelocity = velocity;

        if (jumpRequested && isGrounded)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            isGrounded = false;
        }

        jumpRequested = false;
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Only count surfaces that are beneath the player
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
