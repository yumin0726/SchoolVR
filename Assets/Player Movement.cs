using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    
    [Header("移動參數")]
    public float speed = 5f;
    public float gravity = -15f;

    [Header("跳躍參數")]
    // 可以在 Inspector 中調整的跳躍高度
    public float jumpHeight = 2.0f; 
    
    private Vector3 moveDirection;
    private float velocityY; // 用來處理重力和跳躍的 Y 軸速度

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // 鎖定滑鼠到畫面中心並隱藏
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 
    }

    void Update()
    {
        // 處理滑鼠游標的解除鎖定（可選，方便測試）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 1. 處理水平移動輸入
        float horizontal = Input.GetAxis("Horizontal"); // A/D 輸入
        float vertical = Input.GetAxis("Vertical");     // W/S 輸入

        // 將輸入轉換為相對於角色方向的移動向量
        moveDirection = (transform.right * horizontal) + (transform.forward * vertical);
        moveDirection = moveDirection.normalized * speed;

        // 2. 處理重力與跳躍
        if (controller.isGrounded)
        {
            // 角色在地面上時：
            
            // 重置 Y 軸速度，給予一個小小的負值確保角色貼地
            velocityY = -1f; 

            // 檢查跳躍輸入 (預設使用 Unity 的 "Jump" 輸入，通常是 Space 鍵)
            if (Input.GetButtonDown("Jump")) 
            {
                // 計算跳躍的初始向上速度
                // 公式：v = sqrt(h * -2 * g)
                velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        
        // 不論是否在地面上，每幀都應用重力加速度
        velocityY += gravity * Time.deltaTime;
        
        // 將計算好的 Y 軸速度（重力/跳躍）加入到移動方向中
        moveDirection.y = velocityY;

        // 3. 執行移動
        // CharacterController.Move 需要乘以 Time.deltaTime
        controller.Move(moveDirection * Time.deltaTime);
    }
}