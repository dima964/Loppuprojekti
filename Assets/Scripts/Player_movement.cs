
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

    private float cameraPitch;
    private float cameraYaw;

    private HashSet<Collider> groundColliders =
        new HashSet<Collider>();

    private bool isGrounded => groundColliders.Count > 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Prevent the player from tipping over
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();

            if (cam != null)
                playerCamera = cam.transform;
        }

        cameraYaw = rb.rotation.eulerAngles.y;

        if (playerCamera != null)
        {
            cameraPitch = Mathf.DeltaAngle(
                0f, playerCamera.localEulerAngles.x
            );
        }
    }

    void Update()
    {
        // Disable controls while paused or after game ends
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying)
        {
            moveInput = Vector2.zero;
            jumpRequested = false;
            isSprinting = false;
            return;
        }

        moveInput = Vector2.zero;
        isSprinting = false;

        if (Keyboard.current != null)
        {
            // WASD movement
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

            // Sprint
            isSprinting =
                Keyboard.current.leftShiftKey.isPressed ||
                Keyboard.current.rightShiftKey.isPressed;

            // Interaction
            if (Keyboard.current.eKey.wasPressedThisFrame)
                TryInteract();
        }

        // Read mouse input every frame
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            cameraYaw += mouseDelta.x * mouseSensitivity;
            cameraPitch -= mouseDelta.y * mouseSensitivity;

            cameraPitch = Mathf.Clamp(
                cameraPitch, -85f, 85f
            );
        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying)
            return;

        if (playerCamera == null)
            return;

        // Smooth, frame-based camera rotation
        Quaternion yawRotation =
            Quaternion.Euler(0f, cameraYaw, 0f);

        Quaternion pitchRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);

        playerCamera.rotation = yawRotation * pitchRotation;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying)
            return;

        // Rotate Rigidbody through the physics system
        Quaternion targetRotation =
            Quaternion.Euler(0f, cameraYaw, 0f);

        rb.MoveRotation(targetRotation);

        // Move relative to camera's horizontal direction
        Vector3 movement =
            targetRotation *
            new Vector3(moveInput.x, 0f, moveInput.y);

        movement = movement.normalized;

        float currentSpeed = isSprinting
            ? sprintSpeed
            : moveSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * currentSpeed;
        velocity.z = movement.z * currentSpeed;

        rb.linearVelocity = velocity;

        // Jump
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

    // Interaction
    void TryInteract()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.position,
            playerCamera.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit,
            interactionDistance))
        {
            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
                interactable.Interact();
        }
    }

    // Ground detection
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
}
