using UnityEngine;
using TMPro;
using System.Collections;

// 強制這個物件必須有 RectTransform 組件
[RequireComponent(typeof(RectTransform))]
public class QuestNotification : MonoBehaviour {
    public static QuestNotification Instance;

    [Header("UI 引用")]
    public GameObject notificationPanel; // 整個 Panel 物件
    public TMP_Text notificationText;

    [Header("動畫與停留設定")]
    [Tooltip("通知滑入與滑出的時間長度")]
    public float slideDuration = 0.5f;   
    [Tooltip("通知在螢幕上停留的時間")]
    public float displayDuration = 3.0f; 

    // 重要：動畫曲線，能讓滑動「有快慢節奏」，看起來更順暢。
    // 在 Inspector 可以編輯這個曲線 (EaseInOut 最順手)
    [Tooltip("滑動的曲線 (例如 EaseInOut 會有淡入淡出的感覺)")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform panelRect;
    private Vector2 onScreenPos;  // 面板顯示時的位置 (Inspector 設好的位置)
    private Vector2 offScreenPos; // 面板隱藏時的位置 (滑出螢幕右側)
    private Coroutine activeRoutine; // 紀錄目前正在跑的動畫協程

    void Awake() {
        Instance = this;
        if (notificationPanel == null) return;

        // 1. 抓取 RectTransform
        panelRect = notificationPanel.GetComponent<RectTransform>();

        // 2. 記錄原本在 Inspector 設好的正確位置 (顯示位置)
        onScreenPos = panelRect.anchoredPosition;

        // 3. 計算起點位置：原本 X 位置 + Panel 的寬度 (把整個 Panel 移出螢幕右側)
        // 額外加 50f 保險，確保連邊框都看不見
        offScreenPos = new Vector2(onScreenPos.x + panelRect.rect.width + 50f, onScreenPos.y);

        // 4. 遊戲一開始，把面板放到螢幕外面去
        panelRect.anchoredPosition = offScreenPos;
        notificationPanel.SetActive(false); // 初始隱藏
    }

    public void ShowNotification(string itemName) {
        // 如果上一個動畫還沒跑完，先強制停止，防止位置錯亂
        if (activeRoutine != null) StopCoroutine(activeRoutine);

        // 設定文字
        notificationText.text = $"<color=yellow>任務已完成：</color>\n{itemName}";

        // 開始跑滑動與停留的整套協程
        activeRoutine = StartCoroutine(FullSlideRoutine());
    }

    // 整套流程：滑入 -> 停留 -> 滑出
    IEnumerator FullSlideRoutine() {
        notificationPanel.SetActive(true); // 打開物件開關

        // 1. 滑入：從螢幕外 -> 螢幕內
        Debug.Log("開始滑入...");
        yield return StartCoroutine(MovePanel(offScreenPos, onScreenPos));

        // 2. 停留
        Debug.Log("停留倒數...");
        yield return new WaitForSeconds(displayDuration);

        // 3. 滑出：從螢幕內 -> 螢幕外
        Debug.Log("開始滑出...");
        yield return StartCoroutine(MovePanel(onScreenPos, offScreenPos));

        // 全部跑完，把物件關閉，清空紀錄
        notificationPanel.SetActive(false);
        activeRoutine = null;
    }

    // [通用的移動面板功能] 
    IEnumerator MovePanel(Vector2 startPos, Vector2 endPos) {
        float timer = 0f;
        while (timer < slideDuration) {
            timer += Time.deltaTime; // 累加時間
            
            // 計算進度百分比 (0~1)
            float ratio = timer / slideDuration;

            // 根據動畫曲線計算實際比率 (讓滑動看起來有節奏)
            float lerpValue = slideCurve.Evaluate(ratio);

            // 用 Lerp 插值平滑移動面板位置
            panelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, lerpValue);
            
            yield return null; // 等待下一幀
        }
        // 確保最後位置完全精確 (防止誤差)
        panelRect.anchoredPosition = endPos;
    }
}

