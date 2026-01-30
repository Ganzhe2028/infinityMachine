using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 8f;        // Movement speed
    public LayerMask groundLayer;       // Layer for mouse raycast (very important!)

    private CharacterController _controller;
    private Camera _mainCamera;
    
    // Gravity
    private float _verticalVelocity;
    private const float GRAVITY = -9.81f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    // Handle WASD movement
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float z = Input.GetAxis("Vertical");   // W/S or Up/Down

        // World-relative movement (intuitive for top-down)
        Vector3 move = new Vector3(x, 0, z);

        // Apply gravity (keep grounded)
        if (_controller.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // Small downward force to stay grounded
        }
        _verticalVelocity += GRAVITY * Time.deltaTime;

        // Final movement
        Vector3 finalMove = move * moveSpeed + Vector3.up * _verticalVelocity;
        _controller.Move(finalMove * Time.deltaTime);
    }

    // Handle face-mouse rotation (Top-Down essential)
    void HandleRotation()
    {
        // 1. Cast ray from camera through mouse position
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        // 2. Check if ray hits Ground layer
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            // 3. Get hit point on ground
            Vector3 targetPoint = hit.point;
            
            // 4. Match Y to prevent looking up/down
            targetPoint.y = transform.position.y;

            // 5. Face that point
            transform.LookAt(targetPoint);
        }
    }
}
