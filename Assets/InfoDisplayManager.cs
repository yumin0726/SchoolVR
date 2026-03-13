using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoDisplayManager : MonoBehaviour {
    public static InfoDisplayManager Instance; // 單例模式，方便物品呼叫

    [Header("UI 引用")]
    public GameObject infoPanel;    // 整個看板的物件 (預設先隱藏)
    public Image displayImage;      // 顯示圖片的 UI Image
    public TMP_Text titleText;      // 顯示標題
    public TMP_Text contentText;    // 顯示內容描述

    private AIManager aiManager;    // 引用你之前的 AIManager 來唸聲音

    void Awake() {
        Instance = this;
        aiManager = Object.FindFirstObjectByType<AIManager>();
        infoPanel.SetActive(false); // 遊戲開始先關閉
    }

    public void ShowInfo(ItemInfo info) {
        infoPanel.SetActive(true);
        displayImage.sprite = info.itemImage;
        titleText.text = info.itemName;
        contentText.text = info.description;

        // --- 新增：通知任務系統 ---
        if (TaskManager.Instance != null) {
            TaskManager.Instance.CompleteTask(info.itemName);
        }
        
        Cursor.lockState = CursorLockMode.None; // 解除鎖定
        Cursor.visible = true;

        // 叫老皮唸出這段文字
        if (aiManager != null) {
            // 我們需要把 AIManager 的 DownloadAndPlayVoice 設為 public 才能呼叫
            aiManager.StartCoroutine(aiManager.DownloadAndPlayVoice(info.description));
        }
    }

        // 在 InfoDisplayManager.cs 裡找到這個函式並修改
    public void ClosePanel() {
        infoPanel.SetActive(false); // 關閉看板
        Cursor.lockState = CursorLockMode.Locked; // 鎖定回中央
        Cursor.visible = false;                   // 隱藏滑鼠指針

        // 讓老皮停止說話
        if (aiManager != null && aiManager.voicePlayer != null) {
            aiManager.voicePlayer.Stop();
        }
    }
}
