using UnityEngine;

public class CameraHeadbob : MonoBehaviour
{
    [Header("Normal Headbob Amounts")]
    [SerializeField] private float normalAmount = 0.04f;
    [SerializeField] private float normalFrequency = 15f;
    [SerializeField] private float normalSmooth = 15f;

    [Header("Crouch Headbob Amounts")]
    [SerializeField] private float crouchAmount = 0.04f;
    [SerializeField] private float crouchFrequency = 10f;
    [SerializeField] private float crouchSmooth = 15f;

    [Header("Sprint Headbob Amounts")]
    [SerializeField] private float sprintAmount = 0.06f;
    [SerializeField] private float sprintFrequency = 20f;
    [SerializeField] private float sprintSmooth = 20f;

    private float amount;
    private float frequency;
    private float smooth;

    private Vector3 startPos;
    private PlayerController playerController;

    private void Start()
    {
        startPos = transform.localPosition;
        playerController = GetComponentInParent<PlayerController>();
        SetToNormalAmounts();
    }

    private void Update()
    {
        CheckForHeadbobTrigger();
        StopHeadbob();
    }

    public void SetToNormalAmounts()
    {
        amount = normalAmount;
        frequency = normalFrequency;
        smooth = normalSmooth;
    }

    public void SetToCrouchAmounts()
    {
        amount = crouchAmount;
        frequency = crouchFrequency;
        smooth = crouchSmooth;
    }

    public void SetToSprintAmounts()
    {
        amount = sprintAmount;
        frequency = sprintFrequency;
        smooth = sprintSmooth;
    }

    private void CheckForHeadbobTrigger()
    {
        if (playerController.isMoving && playerController.isGrounded)
        {
            StartHeadbob();
        }
    }

    private Vector3 StartHeadbob()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * frequency) * amount * 1.4f , smooth * Time.deltaTime);
        pos.x += Mathf.Lerp(pos.x, Mathf.Cos(Time.time * frequency / 2f) * amount * 1.6f, smooth * Time.deltaTime);
        transform.localPosition += pos;

        return pos;
    }

    private void StopHeadbob()
    {
        if (transform.localPosition != startPos && !playerController.isCrouching)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, 10 * Time.deltaTime);
        }
    }
}