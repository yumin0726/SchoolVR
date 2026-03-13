using UnityEngine;

public class ItemInfo : MonoBehaviour {
    [Header("物品設定")]
    public string itemName;      // 物品名稱
    [TextArea(3, 10)]
    public string description;   // 物品詳細介紹
    public Sprite itemImage;     // 要顯示的圖片

    // 這個函式必須在類別的大括號裡面！
    private void OnMouseDown() {
        Debug.Log("點擊了物品: " + itemName);
        
        // 檢查 InfoDisplayManager 是否存在，然後傳送資料
        if (InfoDisplayManager.Instance != null) {
            InfoDisplayManager.Instance.ShowInfo(this);
        } else {
            Debug.LogWarning("找不到 InfoDisplayManager！請確認場景中是否有掛載該腳本的物件。");
        }
    }
}