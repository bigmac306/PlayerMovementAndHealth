using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Transform and rigidbody")]
    public Transform playerCamera;
    public Rigidbody body;
    [Space]
    [Header("Player Movement Settings")]
    public float playerSpeed; // keep this value low
    public float maxSpeed;    // set this value around double the player speed
    public float drag = 0.1f;
    public float jumpForce = 5f;
    [Space]
    [Header("Mouse sensitivity and smoothing")]
    public float mouseSensitivity;
    [Space]
    public float mouseSmoothing;
    public float movementSmoothing;
    [Space]
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Space]
    [Header("FOV settings")]
    public int defaultFOV = 90;
    public int sprintFOV = 100;
    public int zoomedFOV = 75;
    public int crouchedFOV = 60;

    private float originalSpeed;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float currentMouseX;
    private float currentMouseY;
    private Vector3 currentVelocity;

    private bool isZoomed = false;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool isGrounded;

    private Camera cam;

    private float moveHorizontal;
    private float moveVertical;
    private float mouseX;
    private float mouseY;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalSpeed = playerSpeed;

        cam = playerCamera.GetComponent<Camera>();
        cam.fieldOfView = defaultFOV;
    }

    public void Update()
    {
        // Variables for inputs
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");

        float scrollInput = Input.mouseScrollDelta.y;

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        int targetFOV = defaultFOV;

        isGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundDistance,
        groundMask
        );

        // Sprinting
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            isSprinting = true;
            playerSpeed = originalSpeed * 1.5f;
        }
        else
        {
            isSprinting = false;
            playerSpeed = originalSpeed;
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            targetFOV = jumpForce > 7f ? sprintFOV : defaultFOV; // Higher jump force gives a more dramatic FOV change
            JumpPlayer();
        }
        else if (isGrounded)
        {
            targetFOV =defaultFOV;
        }
        

        // Move the camera using mouse
        RotatePlayer(mouseX, mouseY);

        // Zoomed in 
        if (scrollInput > 0f)
        {
            isZoomed = true;
        }
        // Zoomed out
        else if (scrollInput < 0f)
        {
            isZoomed = false;
        }

        // Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isSprinting)
        {
            isCrouching = true;
            targetFOV = crouchedFOV;
            PlayerCrouch();
        }

        // Stop crouching
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            targetFOV = defaultFOV;
            FixPlayerCrouchHight();
        }

        // Change FOV is sprinting or zoomed
        if (isSprinting)
        {
            targetFOV = sprintFOV;
        }

        if (isZoomed)
        {
            targetFOV = zoomedFOV;
        }

        SetFOV(targetFOV);
    }

    public void FixedUpdate()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 movement = (forward * moveVertical + right * moveHorizontal).normalized;

        // Move the player (WASD)
        MovePlayer(movement);
    }

    public void PlayerCrouch()
    {
        transform.localScale = new Vector3(1, 0.5f, 1);
        playerSpeed = originalSpeed / 2;
    }

    public void FixPlayerCrouchHight()
    {
        transform.localScale = new Vector3(1, 1, 1);
        playerSpeed = originalSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }

    

    public void RotatePlayer(float mouseX, float mouseY)
    {
        currentMouseX = Mathf.Lerp(
            currentMouseX,
            mouseX,
            mouseSmoothing * Time.deltaTime
        );

        currentMouseY = Mathf.Lerp(
            currentMouseY,
            mouseY,
            mouseSmoothing * Time.deltaTime
        );

        xRotation -= currentMouseY * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += currentMouseX * mouseSensitivity;

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void SetFOV(int fov)
    {
        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            fov,
            Time.deltaTime * 10f
        );
    }

    public void JumpPlayer()
    {
        body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void MovePlayer(Vector3 movement)
    {
        Vector3 targetVelocity = movement * playerSpeed;

        targetVelocity.y = body.linearVelocity.y;
        drag = isSprinting ? 0.1f : 0.05f; 

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            movementSmoothing * Time.deltaTime
        );

        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -maxSpeed, maxSpeed);
        currentVelocity.z = Mathf.Clamp(currentVelocity.z, -maxSpeed, maxSpeed);

        body.linearVelocity = currentVelocity;
    }
}