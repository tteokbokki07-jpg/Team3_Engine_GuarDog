using Unity.VisualScripting;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpPower = 5f;
    public float Gravity = 20f;
    public float mouseSensitivity = 0.2f;
    public Transform cameraPivot;
    public Transform cameraTransform;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float pitch = 20f;
    private float verticalVelocity;
    private CharacterController controller;
    
    // 입력 허용 여부
    public bool allowInput = true;
    // 카메라 입력 허용 여부
    public bool allowLook = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // 게임 시작 시 커서 잠금 및 숨김
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetInputEnabled(true);
        allowInput = true;
        allowLook = true;
    }
    public void SetInputEnabled(bool enabled)
    {
        allowInput = enabled;
        allowLook = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isRunning = false;
        }
    }
    public void OnMove(InputValue value)
    {
        if (!allowInput)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (!allowInput)
        {
            lookInput = Vector2.zero;
            return;
        }

        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!allowInput)
            return;

        if (value.isPressed && controller.isGrounded)
        {
            verticalVelocity = jumpPower;
        }
    }

    public void OnSprint(InputValue value)
    {
        if (!allowInput)
        {
            isRunning = false;
            return;
        }

        isRunning = value.isPressed;
    }

    // 외부에서 카메라 입력 허용/차단
    public void SetLookEnabled(bool enabled)
    {
        allowLook = enabled;
    }

    void Update()
    {
        if (allowLook)
        {
            transform.Rotate(
                0f,
                lookInput.x * mouseSensitivity,
                0f
            );

            pitch -= lookInput.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -20f, 60f);
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += Gravity * Time.deltaTime;

        Vector3 move = Vector3.zero;

        if (allowInput)
        {
            move = transform.forward * moveInput.y
                 + transform.right * moveInput.x;
        }

        float speed = moveSpeed;
        float targetZ = -6f;

        if (allowInput && isRunning)
        {
            speed = moveSpeed * 2f;
            targetZ = -8f;
        }

        move *= speed;
        move.y = verticalVelocity;

        Vector3 camPos = cameraTransform.localPosition;
        camPos.z = Mathf.Lerp(
            camPos.z,
            targetZ,
            5f * Time.deltaTime
        );
        cameraTransform.localPosition = camPos;

        controller.Move(move * Time.deltaTime);
    }
}
