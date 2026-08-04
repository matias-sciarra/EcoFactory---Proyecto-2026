using UnityEngine;

/// <summary>
/// Controlador FPS en primera persona para Unity 2018.
/// Requiere un CharacterController en el mismo GameObject.
/// La cámara debe ser hija del jugador (ver instrucciones abajo).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f; // -9.81 * 2, se siente mejor en juegos

    [Header("Mirada / Cámara")]
    public Camera playerCamera;
    public float mouseSensitivity = 2.5f;
    public float maxLookAngle = 85f;
    public bool invertY = false;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Head Bob (opcional)")]
    public bool enableHeadBob = true;
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.05f;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;
    private float defaultCameraY;
    private float bobTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            defaultCameraY = playerCamera.transform.localPosition.y;
        }
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMouseLook();
        HandleMovement();
        HandleHeadBob();

        // Permitir liberar el cursor con Escape (útil para pausar/testear)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    void HandleGroundCheck()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // pequeña fuerza para mantenerlo pegado al suelo
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        // Rotar el cuerpo del jugador en el eje horizontal (Y)
        transform.Rotate(Vector3.up * mouseX);

        // Rotar la cámara en el eje vertical (X), con límite
        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        if (!enableHeadBob || playerCamera == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f) && isGrounded;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = defaultCameraY + bobOffset;
            playerCamera.transform.localPosition = camPos;
        }
        else
        {
            bobTimer = 0f;
            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, defaultCameraY, Time.deltaTime * 5f);
            playerCamera.transform.localPosition = camPos;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
