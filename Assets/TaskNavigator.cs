using UnityEngine;
using UnityEngine.AI;

public class TaskNavigator : MonoBehaviour {
    public static TaskNavigator Instance;

    [Header("引用設定")]
    public LineRenderer line; 
    public Transform currentTarget; 

    [Header("外觀微調")]
    public float heightOffset = 0.5f;   // 墊高的高度
    public float scrollSpeed = 2.0f;   // 箭頭流動的速度
    
    private NavMeshPath path;
    private Material lineMaterial;     // 用來控制貼圖流動的材質球

    void Awake() {
        Instance = this;
        path = new NavMeshPath();
        
        if (line != null) {
            line.enabled = false;
            lineMaterial = line.material; // 抓取 LineRenderer 上的材質球
        }
    }

    void Update() {
        // 如果有目標且導航開啟，就計算路徑並讓貼圖「流動」
        if (currentTarget != null && line != null && line.enabled) {
            DrawPath();
            AnimateLine(); // 讓箭頭跑起來的功能
        }
    }

    public void StartNavigation(Transform target) {
        if (target == null) return;
        currentTarget = target;
        line.enabled = true;
    }

    public void StopNavigation() {
        if (line != null) line.enabled = false;
        currentTarget = null;
    }

    void DrawPath() {
        // 1. 計算路徑
        bool hasPath = NavMesh.CalculatePath(transform.position, currentTarget.position, NavMesh.AllAreas, path);

        // 2. 檢查路徑有效性
        if (!hasPath || path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2) {
            line.positionCount = 0;
            return;
        }

        // 3. 設定頂點
        line.positionCount = path.corners.Length;
        for (int i = 0; i < path.corners.Length; i++) {
            Vector3 point = path.corners[i];
            point.y += heightOffset; // 使用變數，方便你在 Inspector 調整
            line.SetPosition(i, point);
        }
    }

    // --- 新增：讓箭頭貼圖移動的效果 ---
    void AnimateLine() {
        if (lineMaterial != null) {
            // 讓材質球的 Texture Offset 隨著時間改變，產生箭頭往前跑的感覺
            float offset = Time.time * scrollSpeed;
            // 注意：如果你的箭頭貼圖方向相反，就把 offset 改成 -offset
            lineMaterial.mainTextureOffset = new Vector2(-offset, 0);
        }
    }
}
