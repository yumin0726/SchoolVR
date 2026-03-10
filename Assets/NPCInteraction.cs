using UnityEngine;

public class NPCInteraction : MonoBehaviour {
    public GameObject dialoguePanel; // 拖入你的 UI 面板

    void Update() {
        if (Input.GetMouseButtonDown(0)) { // 偵測左鍵點擊
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.CompareTag("NPC")) { // 確保角色 Tag 設為 NPC
                    OpenDialogue();
                }
            }
        }
    }

    void OpenDialogue() {
        dialoguePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // 釋放滑鼠
    }
}