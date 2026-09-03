using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckOffset = 0.1f;

    [Header("Mouse / Camera")]
    [SerializeField] private Transform playerCamera; // assign camera (recommended) or child camera is used
    [SerializeField] private float lookSensitivity = 2.0f;
    [SerializeField] private float verticalLookLimit = 80f;
    [SerializeField] private bool lockCursor = true;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch; // rotation X (up/down)
    private Vector2 currentMouseDelta;
    private Vector2 smoothMouseVelocity;
    [SerializeField] private float mouseSmoothTime = 0.03f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCursorToggle();
    }

    private void HandleMouseLook()
    {
        Vector2 rawMouse = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        // smooth mouse
        currentMouseDelta.x = Mathf.SmoothDamp(currentMouseDelta.x, rawMouse.x, ref smoothMouseVelocity.x, mouseSmoothTime);
        currentMouseDelta.y = Mathf.SmoothDamp(currentMouseDelta.y, rawMouse.y, ref smoothMouseVelocity.y, mouseSmoothTime);

        // yaw (player rotation)
        transform.Rotate(Vector3.up * currentMouseDelta.x * lookSensitivity);

        // pitch (camera up/down)
        cameraPitch -= currentMouseDelta.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);

        if (playerCamera != null)
            playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }

    private void HandleMovement()
    {
        // Determine speeds and input
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float inputX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float inputZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Movement relative to player forward
        Vector3 move = transform.right * inputX + transform.forward * inputZ;
        move = move.normalized * speed;

        // Grounded and gravity/jump
        if (controller.isGrounded && verticalVelocity < 0)
        {
            // small downward offset so CharacterController remains grounded
            verticalVelocity = -2f;
        }

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            // v = sqrt(2 * g * h)
            verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Optional: visualize ground check in editor
    void OnDrawGizmosSelected()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position + controller.center + Vector3.down * (controller.height / 2 - controller.radius);
            Gizmos.DrawWireSphere(center, controller.radius + groundCheckOffset);
        }
    }
}
