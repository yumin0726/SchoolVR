using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour {
    public static TaskManager Instance;

    [Header("UI 設定")]
    public GameObject taskPanel;         // 任務清單的母物件
    public TMP_Text taskListText;        // 顯示任務文字的地方

    [Header("音效設定")]
    public AudioSource audioSource;    
    public AudioClip successSound;     

    [Header("任務內容")]
    // 這裡儲存：任務名稱 與 是否完成
    private Dictionary<string, bool> tasks = new Dictionary<string, bool>();

    void Awake() {
        Instance = this;
        // 初始化任務 (注意：這裡的名字必須跟場景中 ItemInfo 的 itemName 完全一模一樣)
        tasks.Add("鳳凰于飛", false);
        tasks.Add("宜大校門", false);
        
        UpdateTaskUI();
        taskPanel.SetActive(false); 

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            ToggleTaskPanel();
        }
    }

    public void ToggleTaskPanel() {
        bool isActive = !taskPanel.activeSelf;
        taskPanel.SetActive(isActive);

        if (isActive) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            if (InfoDisplayManager.Instance != null && !InfoDisplayManager.Instance.infoPanel.activeSelf) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // 當任務點擊時觸發導航 (請將 UI 按鈕連動到這裡)
    public void StartNavToTask(string taskName) {
        Debug.Log($"<color=cyan>TaskManager: 開始尋找導航目標 -> {taskName}</color>");

        // 1. 尋找場景中所有的 ItemInfo
        ItemInfo[] allItems = Object.FindObjectsByType<ItemInfo>(FindObjectsSortMode.None);
        
        bool foundItem = false;

        foreach (var item in allItems) {
            // 檢查名字 (去除空白並比對)
            if (item.itemName.Trim() == taskName.Trim()) {
                foundItem = true;

                // 2. 檢查導航目標物件是否存在
                if (item.navTarget != null) {
                    if (TaskNavigator.Instance != null) {
                        TaskNavigator.Instance.StartNavigation(item.navTarget);
                        Debug.Log($"<color=green>成功：找到 {taskName}，已啟動導航線！</color>");
                        ToggleTaskPanel(); // 關閉清單看路
                    } else {
                        Debug.LogError("錯誤：場景中找不到 TaskNavigator 實例！");
                    }
                } else {
                    Debug.LogWarning($"警告：找到物品 {taskName}，但它身上的 'Nav Target' 格子是空的！");
                }
                break;
            }
        }

        if (!foundItem) {
            Debug.LogError($"錯誤：在場景中完全找不到名為 '{taskName}' 的物品，請檢查名字是否拼錯。");
        }
    }

    public void CompleteTask(string itemName) {
        if (tasks.ContainsKey(itemName) && tasks[itemName] == false) {
            tasks[itemName] = true;
            UpdateTaskUI();

            if (audioSource != null && successSound != null) {
                audioSource.PlayOneShot(successSound);
            }

            if (QuestNotification.Instance != null) {
                QuestNotification.Instance.ShowNotification(itemName);
            }
            
            // 任務完成後，自動關閉該物品的導航線
            if (TaskNavigator.Instance != null) {
                TaskNavigator.Instance.StopNavigation();
            }

            Debug.Log($"<color=yellow>任務正式完成：{itemName}</color>");
        }
    }

    void UpdateTaskUI() {
        if (taskListText == null) return;
        taskListText.text = "<b>校園探險任務</b>\n\n";
        foreach (var task in tasks) {
            string status = task.Value ? "<s><color=green>[已完成]</color></s>" : "<color=red>[未完成]</color>";
            taskListText.text += $"{status} {task.Key}\n";
        }
    }
}