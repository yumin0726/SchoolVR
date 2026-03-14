using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoDisplayManager : MonoBehaviour {
    public static InfoDisplayManager Instance;

    [Header("UI 引用")]
    public GameObject infoPanel;    
    public Image displayImage;      
    public TMP_Text titleText;      
    public TMP_Text contentText;    

    private AIManager aiManager;    
    private string currentItemName; 

    void Awake() {
        Instance = this;
        aiManager = Object.FindFirstObjectByType<AIManager>();
        infoPanel.SetActive(false); 
    }

    // 1. 打開面板：只負責顯示與「記錄名字」
    public void ShowInfo(ItemInfo info) {
        infoPanel.SetActive(true);
        displayImage.sprite = info.itemImage;
        titleText.text = info.itemName;
        contentText.text = info.description;

        // --- 關鍵修改：這裡「不」呼叫 TaskManager，只把名字存起來 ---
        currentItemName = info.itemName; 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (aiManager != null) {
            aiManager.StartCoroutine(aiManager.DownloadAndPlayVoice(info.description));
        }
    }

    // 2. 關閉面板：這時候才正式通知任務完成
    public void ClosePanel() {
        infoPanel.SetActive(false); 

        // --- 核心改動：按下關閉鈕時，才拿剛才存的名字去完成任務 ---
        if (!string.IsNullOrEmpty(currentItemName) && TaskManager.Instance != null) {
            TaskManager.Instance.CompleteTask(currentItemName);
            currentItemName = ""; // 發送完後清空暫存，避免重複觸發
        }

        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;                   

        if (aiManager != null && aiManager.voicePlayer != null) {
            aiManager.voicePlayer.Stop();
        }
    }
}