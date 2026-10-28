
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.1f;

    [Header("Interaction")]
    public float interactionDistance = 3f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpRequested;
    private bool isSprinting;

    private float cameraPitch = 0f;

    private HashSet<Collider> groundColliders =
        new HashSet<Collider>();

    private bool isGrounded => groundColliders.Count > 0;
    private bool isPaused = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();

            if (cam != null)
                playerCamera = cam.transform;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Pause / Resume with ESC
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Don't allow player input while paused
        if (isPaused)
        {
            moveInput = Vector2.zero;
            jumpRequested = false;
            isSprinting = false;
            return;
        }
        moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            // Movement
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

            // Jump
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpRequested = true;

            // Sprint while Shift is held
            isSprinting =
                Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed;

            // Interact when E is pressed
            if (Keyboard.current.eKey.wasPressedThisFrame)
                TryInteract();
        }

        // Mouse look
        if (Mouse.current != null && playerCamera != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

            playerCamera.localRotation = Quaternion.Euler(
                cameraPitch, 0f, 0f
            );
        }
    }

    void FixedUpdate()
    {
        Vector3 movement =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        movement = movement.normalized;

        // Choose normal or sprint speed
        float currentSpeed = isSprinting
            ? sprintSpeed
            : moveSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * currentSpeed;
        velocity.z = movement.z * currentSpeed;

        rb.linearVelocity = velocity;

        if (jumpRequested && isGrounded)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            groundColliders.Clear();
        }

        jumpRequested = false;
    }

    // Interaction system
    void TryInteract()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.position,
            playerCamera.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckGround(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckGround(collision);
    }

    void CheckGround(Collision collision)
    {
        bool validGround = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                validGround = true;
                break;
            }
        }

        if (validGround)
            groundColliders.Add(collision.collider);
        else
            groundColliders.Remove(collision.collider);
    }

    void OnCollisionExit(Collision collision)
    {
        groundColliders.Remove(collision.collider);
    }


    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pause game
            Time.timeScale = 0f;

            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Game Paused");
        }
        else
        {
            // Resume game
            Time.timeScale = 1f;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Game Resumed");
        }
    }

}
