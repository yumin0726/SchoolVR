using UnityEngine;

// [重要] 自動幫物件加上碰撞體偵測，沒這行滑鼠點不到物品！
[RequireComponent(typeof(Collider))]
public class ItemInfo : MonoBehaviour {
    [Header("物品設定")]
    [Tooltip("此名稱必須與 TaskManager 任務清單中的名稱完全一致")]
    public string itemName;      // 物品名稱

    public Transform navTarget; // 在 Inspector 裡把該建築物前方的「空物件」拉進來
    
    [TextArea(3, 10)]
    public string description;   // 物品詳細介紹 (老皮會根據這段唸稿)
    
    public Sprite itemImage;     // 看板上要顯示的物品圖片

    // 當滑鼠點擊這個 3D 物件時觸發
    private void OnMouseDown() {
        // 如果目前看板已經開著，可以選擇不重複觸發 (可選)
        if (InfoDisplayManager.Instance != null && InfoDisplayManager.Instance.infoPanel.activeSelf) 
            return;

        Debug.Log($"<color=yellow>點擊了物品：{itemName}</color>");
        
        // 檢查看板管理員是否存在
        if (InfoDisplayManager.Instance != null) {
            // 只負責傳送資料並打開面板，完成任務的動作交給 ClosePanel 去做
            InfoDisplayManager.Instance.ShowInfo(this);
        } else {
            Debug.LogError("找不到 InfoDisplayManager！請確認場景中（通常在 Canvas 下）有掛載該腳本的物件。");
        }
    }
}