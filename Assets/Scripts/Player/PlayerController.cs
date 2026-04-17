using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 24f;
    [SerializeField] private float sprintStaminaPerSecond = 12f;
    [SerializeField] private float staminaRecoverPerSecond = 18f;
    [SerializeField] private float lookSensitivity = 2f;

    private CharacterController _controller;
    private PlayerStats _stats;
    private Animator _animator;
    private float _verticalVelocity;
    private Vector3 _horizontalVelocity;
    private float _pitch;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _stats = GetComponent<PlayerStats>();
        _animator = GetComponentInChildren<Animator>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        UpdateLook();
        UpdateMovement();
        UpdateAnimations();
    }

    private void UpdateLook()
    {
        if (playerCamera == null)
        {
            return;
        }

        var mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        _pitch = Mathf.Clamp(_pitch - mouseY, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void UpdateMovement()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        var wantsSprint = Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0.1f;
        var canSprint = wantsSprint && _stats.Stamina > 0;
        var targetSpeed = canSprint ? sprintSpeed : walkSpeed;

        if (canSprint)
        {
            _stats.SpendStamina(Mathf.CeilToInt(sprintStaminaPerSecond * Time.deltaTime));
        }
        else
        {
            _stats.RestoreStamina(Mathf.CeilToInt(staminaRecoverPerSecond * Time.deltaTime));
        }

        var desired = transform.TransformDirection(input) * targetSpeed;
        var lerpRate = input.sqrMagnitude > 0.01f ? acceleration : deceleration;
        _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, desired, lerpRate * Time.deltaTime);

        if (_controller.isGrounded)
        {
            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            if (Input.GetButtonDown("Jump"))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        _verticalVelocity += gravity * Time.deltaTime;

        var move = _horizontalVelocity;
        move.y = _verticalVelocity;
        _controller.Move(move * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        if (_animator == null)
        {
            return;
        }

        var planarSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
        _animator.SetFloat("MoveSpeed", planarSpeed);
        _animator.SetBool("Grounded", _controller.isGrounded);
    }
}
