using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpHeight = 1.2f;
    private float gravity = -9.8f;
    [NonSerialized] public bool isGrounded;
    [NonSerialized] public bool isMoving;
    private bool isCrouching;
    private Vector3 velocity;
    private Coroutine crouchCoroutine;

    [Header("Look")]
    [SerializeField] private Camera cam;
    private CameraHeadbob cameraHeadbob;
    private float sensitivity = 2.5f;
    private float xRotation = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraHeadbob = cam.GetComponent<CameraHeadbob>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        Move();
        Look();
        Crouch();
    }

    private void Move()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(moveSpeed * Time.deltaTime * move);

        if (x != 0 || z != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            HandleCrouch(new Vector3(0, 0.25f, 0), 1, 2f, true);
            cameraHeadbob.SetToCrouchAmounts();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            HandleCrouch(new Vector3(0, 0.53f, 0), 2, 6f, false);
            cameraHeadbob.SetToNormalAmounts();
        }
    }

    private void HandleCrouch(Vector3 _endPosition, float _height, float _moveSpeed, bool _isCrouching)
    {
        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
        }

        crouchCoroutine = StartCoroutine(SmoothTransition(cam.transform.localPosition, _endPosition, 0.2f));
        characterController.height = _height;
        moveSpeed = _moveSpeed;
        isCrouching = _isCrouching;
    }

    private IEnumerator SmoothTransition(Vector3 start, Vector3 end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            cam.transform.localPosition = Vector3.Lerp(start, end, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.transform.localPosition = end;
    }
}