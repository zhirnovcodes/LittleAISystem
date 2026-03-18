using UnityEngine;

public class FreeFlyCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private bool lockCursorOnStart = true;

    private float _pitch;
    private float _yaw;

    private void Start()
    {
        Vector3 currentRotation = transform.eulerAngles;
        _pitch = currentRotation.x;
        _yaw = currentRotation.y;

        if (lockCursorOnStart)
        {
            LockCursor(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }

        if (lockCursorOnStart && Input.GetMouseButtonDown(0))
        {
            LockCursor(true);
        }

        UpdateRotation();
        UpdateMovement();
    }

    private void UpdateRotation()
    {
        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void UpdateMovement()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            input += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            input += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A))
        {
            input += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            input += Vector3.right;
        }

        if (Input.GetKey(KeyCode.E))
        {
            input += Vector3.up;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            input += Vector3.down;
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        float currentMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMoveSpeed *= sprintMultiplier;
        }

        Vector3 movement =
            transform.forward * input.z +
            transform.right * input.x +
            transform.up * input.y;

        transform.position += movement * (currentMoveSpeed * Time.deltaTime);
    }

    private static void LockCursor(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
