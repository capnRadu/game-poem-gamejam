using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeedAddition = 4f;
    [SerializeField] private float crouchSpeedAddition = -4f;
    [SerializeField] private float jumpHeight = 1.2f;

    [SerializeField] private float state2SpeedAddition = 2f;
    [SerializeField] private float state3SpeedAddition = -4f;
    [SerializeField] private float state4SpeedAddition = 2f;

    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 67f;

    [NonSerialized] public bool isGrounded;
    [NonSerialized] public bool isMoving;
    [NonSerialized] public bool isCrouching;

    private bool isSprinting;
    private bool isSprintLocked = true;
    private float gravity = -9.8f;
    private Vector3 velocity;
    private Coroutine crouchCoroutine;
    private Coroutine sprintCoroutine;
    [SerializeField] private AudioSource footstepSound;

    [Header("Look")]
    [SerializeField] private Camera cam;
    private CameraHeadbob cameraHeadbob;
    private float sensitivity = 2.5f;
    private float xRotation = 0f;

    [Header("Interaction")]
    [SerializeField] private float lookDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactText;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraHeadbob = cam.GetComponent<CameraHeadbob>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveSpeed = walkSpeed;
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        Move();
        Look();
        Crouch();
        Sprint();
        Footsteps();
        CheckForInteractable();
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
        if (!isSprinting)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                HandleCrouch(new Vector3(0, 0.25f, 0), 1, moveSpeed + crouchSpeedAddition, true);
                cameraHeadbob.SetToCrouchAmounts();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                HandleCrouch(new Vector3(0, 0.53f, 0), 2, walkSpeed, false);
                cameraHeadbob.SetToNormalAmounts();
            }
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

    private void Sprint()
    {
        if (!isCrouching && !isSprintLocked)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && isMoving)
            {
                HandleSprint(moveSpeed + sprintSpeedAddition, sprintFOV, true);
                cameraHeadbob.SetToSprintAmounts();
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                HandleSprint(walkSpeed, normalFOV, false);
                StartCoroutine(SmoothFovChange(normalFOV));
            }
        }
    }

    private void HandleSprint(float _moveSpeed, float _fov, bool _isSprinting)
    {
        if (sprintCoroutine != null)
        {
            StopCoroutine(sprintCoroutine);
        }

        sprintCoroutine = StartCoroutine(SmoothFovChange(_fov));
        moveSpeed = _moveSpeed;
        isSprinting = _isSprinting;
    }

    private IEnumerator SmoothFovChange(float targetFOV)
    {
        float startFOV = cam.fieldOfView;
        float elapsedTime = 0f;

        while (elapsedTime < 0.2f)
        {
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, (elapsedTime / 0.2f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.fieldOfView = targetFOV;
    }

    private void Footsteps()
    {
        if (isGrounded && isMoving)
        {
            if (!footstepSound.isPlaying)
            {
                if (!isCrouching)
                {
                    StartCoroutine(FootstepSound(0.08f));
                }
                else
                {
                    StartCoroutine(FootstepSound(0.15f));
                }
            }
        }
    }

    private IEnumerator FootstepSound(float time)
    {
        yield return new WaitForSeconds(time);
        footstepSound.pitch = UnityEngine.Random.Range(0.8f, 1.1f);
        footstepSound.Play();
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lookDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Pickable"))
            {
                interactText.text = "Press E to pick up";
                interactText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Destroy(hit.collider.gameObject);
                    interactText.gameObject.SetActive(false);
                    StageManager.Instance.CollectPage();
                }
            }            
        }
        else
        {
            interactText.gameObject.SetActive(false);
        }
    }

    public void UpdateSpeed(string state)
    {
        switch (state)
        {
            case "State2":
                walkSpeed += state2SpeedAddition;
                break;
            case "State3":
                walkSpeed += state3SpeedAddition;
                break;
            case "State4":
                walkSpeed += state4SpeedAddition;
                isSprintLocked = false;
                break;
        }

        moveSpeed = walkSpeed;
    }
}