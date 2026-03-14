using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour {
    public static TaskManager Instance;

    [Header("UI 設定")]
    public GameObject taskPanel;         // 任務清單的母物件
    public TMP_Text taskListText;        // 顯示任務文字的地方

    [Header("音效設定")]
    public AudioSource audioSource;    // 拖入一個 AudioSource 組件
    public AudioClip successSound;     // 拖入你的「叮！」音效檔

    [Header("任務內容")]
    // 這裡我們用 Dictionary 來存：任務名稱 與 是否完成
    private Dictionary<string, bool> tasks = new Dictionary<string, bool>();

    void Awake() {
        Instance = this;
        // 初始化一些任務，名字要跟 ItemInfo 裡的 Item Name 一模一樣
        tasks.Add("鳳凰于飛", false);
        tasks.Add("宜大校門", false);
        
        UpdateTaskUI();
        taskPanel.SetActive(false); // 初始隱藏
        // 如果沒手動掛載 AudioSource，自動幫忙抓一個
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        // 按下 E 鍵切換開關
        if (Input.GetKeyDown(KeyCode.E)) {
            ToggleTaskPanel();
        }
    }

    void ToggleTaskPanel() {
        bool isActive = !taskPanel.activeSelf;
        taskPanel.SetActive(isActive);

        // 如果打開清單，解鎖滑鼠；關閉則鎖定（視你的移動模式而定）
        if (isActive) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            // 如果沒有其他視窗打開，才鎖回滑鼠
            if (!InfoDisplayManager.Instance.infoPanel.activeSelf) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // 當看過物品並「關閉面板」時，由 InfoDisplayManager 呼叫
    public void CompleteTask(string itemName) {
        // 檢查點：1. 任務清單裡有這個名字 2. 該任務目前的狀態是「未完成」(false)
        if (tasks.ContainsKey(itemName) && tasks[itemName] == false) {
        
            tasks[itemName] = true; // 正式標記為已完成
            UpdateTaskUI();         // 讓清單上的文字打勾/變色

            // --- 播放成功音效 (只有第一次完成會響) ---
            if (audioSource != null && successSound != null) {
                audioSource.PlayOneShot(successSound);
            }

            // --- 2. 新增：顯示右上角通知 ---
            if (QuestNotification.Instance != null) {
                QuestNotification.Instance.ShowNotification(itemName);
            }
        
            Debug.Log($"<color=green>叮！任務首度完成：{itemName}</color>");

        } else if (tasks.ContainsKey(itemName) && tasks[itemName] == true) {
            // 如果已經完成過了，就只在後台記錄，不撥音效，也不重複更新 UI
            Debug.Log($"{itemName} 之前就做過了，不再重複播放音效。");
        }
    }

    // 更新介面上的文字
    void UpdateTaskUI() {
        taskListText.text = "<b>校園探險任務</b>\n\n";
        foreach (var task in tasks) {
            string status = task.Value ? "<s><color=green>[已完成]</color></s>" : "<color=red>[未完成]</color>";
            taskListText.text += $"{status} {task.Key}\n";
        }
    }
}