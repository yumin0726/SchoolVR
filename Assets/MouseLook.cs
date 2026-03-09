using UnityEngine;

public class MouseLook : MonoBehaviour
{
    // 調高這個數值可以加快轉動速度
    [Range(100f, 1000f)]
    public float mouseSensitivity = 300f;

    public Transform playerBody;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 取得滑鼠輸入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 上下轉動
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 左右轉動
        playerBody.Rotate(Vector3.up * mouseX);
    }
}