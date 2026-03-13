using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour {
    public static TaskManager Instance;

    [Header("UI 設定")]
    public GameObject taskPanel;         // 任務清單的母物件
    public TMP_Text taskListText;        // 顯示任務文字的地方

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

    // 當看過物品時，呼叫這個函式
    public void CompleteTask(string itemName) {
        if (tasks.ContainsKey(itemName)) {
            tasks[itemName] = true;
            UpdateTaskUI();
            Debug.Log($"任務完成：{itemName}");
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