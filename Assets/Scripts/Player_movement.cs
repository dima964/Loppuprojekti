
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player_movement : MonoBehaviour
{
    [Header("Movement")] //These are tags so that it is easier to locatie the stuff instead of surfing through everything.
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 1.1f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.1f;

    [Header("Interaction")]
    public float interactionDistance = 3f;

    private Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;

    private float cameraYaw;
    private float cameraPitch;

    private bool isSprinting;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Make physics movement look smoother
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Stop the player from falling sideways
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        // Find the camera
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();

            if (cam != null)
            {
                playerCamera = cam.transform;
            }
        }

        cameraYaw = transform.eulerAngles.y;

        if (playerCamera != null)
        {
            cameraPitch = playerCamera.localEulerAngles.x;

            if (cameraPitch > 180f)
            {
                cameraPitch -= 360f;
            }
        }
    }

    void Update()
    {
        // Check if the game is paused or finished
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsPlaying == false)
            {
                horizontalInput = 0f;
                verticalInput = 0f;
                isSprinting = false;
                jumpRequested = false;
                return;
            }
        }

        // Keyboard controls
        if (Keyboard.current != null)
        {
            horizontalInput = 0f;
            verticalInput = 0f;

            // Forward
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                verticalInput += 1f;
            }

            // Backward
            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
            {
                verticalInput -= 1f;
            }

            // Right
            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                horizontalInput += 1f;
            }

            // Left
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                horizontalInput -= 1f;
            }

            // Sprint with Shift
            isSprinting = false;

            if (Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed)
            {
                isSprinting = true;
            }

            // Jump with Space
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
            }

            // Interact with E
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }
        }

        // Mouse controls
        if (Mouse.current != null)
        {
            Vector2 mouseMovement =
                Mouse.current.delta.ReadValue();

            cameraYaw += mouseMovement.x * mouseSensitivity;
            cameraPitch -= mouseMovement.y * mouseSensitivity;

            // Stop camera from looking too far up or down
            cameraPitch = Mathf.Clamp(
                cameraPitch, -85f, 85f
            );
        }
    }

    void LateUpdate()
    {
        // Don't move camera when paused
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsPlaying == false)
            {
                return;
            }
        }

        if (playerCamera == null)
        {
            return;
        }

        // Rotate the camera smoothly
        playerCamera.rotation = Quaternion.Euler(
            cameraPitch, cameraYaw, 0f
        );
    }

    void FixedUpdate()
    {
        // Stop physics movement when game isn't playing
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsPlaying == false)
            {
                return;
            }
        }

        // Turn player left and right
        Quaternion playerRotation =
            Quaternion.Euler(0f, cameraYaw, 0f);

        rb.MoveRotation(playerRotation);

        // Calculate movement direction
        Vector3 movement = new Vector3(
            horizontalInput, 0f, verticalInput
        );

        movement = playerRotation * movement;
        movement.Normalize();

        // Choose walking or sprinting speed
        float speed = moveSpeed;

        if (isSprinting == true)
        {
            speed = sprintSpeed;
        }

        // Move player
        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * speed;
        velocity.z = movement.z * speed;

        rb.linearVelocity = velocity;

        // Check if there is ground below the player
        bool isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance
        );

        // Jump only when touching the ground
        if (jumpRequested == true && isGrounded == true)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
        }

        jumpRequested = false;
    }

    // Interacting with objects
    void TryInteract()
    {
        if (playerCamera == null)
        {
            return;
        }

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
}
