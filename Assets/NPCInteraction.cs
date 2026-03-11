using UnityEngine;

public class NPCInteraction : MonoBehaviour {
    public GameObject dialoguePanel; // 拖入你的對話面板
    public float interactDistance = 5f; // 設定多近才能點到

    void Update() {
        // 偵測滑鼠左鍵點擊
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 射出一道隱形射線
            if (Physics.Raycast(ray, out hit, interactDistance)) {
                // 檢查打到的物件標籤是不是 NPC
                if (hit.collider.CompareTag("NPC")) {
                    OpenDialogue();
                }
            }
        }

        // 額外小功能：按 Esc 可以快速關閉視窗
        if (Input.GetKeyDown(KeyCode.Escape)) {
            CloseDialogue();
        }
    }

    void OpenDialogue() {
        dialoguePanel.SetActive(true);
        // 讓滑鼠游標顯示出來（如果你的遊戲有鎖定滑鼠的話）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseDialogue() {
        dialoguePanel.SetActive(false);
    }
}