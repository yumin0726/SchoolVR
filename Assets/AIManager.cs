using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Newtonsoft.Json; // 確保有這行

public class AIManager : MonoBehaviour {
    [Header("API 設定")]
    [SerializeField] private string apiKey = "AIzaSyAsRulNQWl-PiDbOHbL72Kj1V9WUnhQU4g";
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("NPC 設定")]
    [TextArea(3, 5)]
    public string npcPersonality = "你是一個幽默的寶藏獵人，名字叫老皮。";

    [Header("UI 引用")]
    public TMP_InputField inputField;
    public TMP_Text responseText;
    public UnityEngine.UI.Button sendButton;

    // 重點修改：這裡改用 Content，對應 GeminiData.cs
    private List<Content> chatHistory = new List<Content>();

    void Start() {
        // 這裡是關鍵：直接把個性設定當成使用者的第一句話，讓 AI 了解它的角色
        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = $"從現在起，你的個性設定如下：{npcPersonality}。請以此身分開始對話。" } }
        });
    
        // 給 AI 一個模擬回覆，確認它接收到了
        chatHistory.Add(new Content {
            role = "model",
            parts = new List<Part> { new Part { text = "沒問題，我已經準備好以此身分與你交流了！請問有什麼事嗎？" } }
        });
    }

    public void OnSendClick() {
        if (string.IsNullOrEmpty(inputField.text)) return;

        // 記錄玩家的話
        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = inputField.text } }
        });

        StartCoroutine(PostToGemini());
        inputField.text = "";
    }

    IEnumerator PostToGemini() {
        responseText.text = "史蒂夫正在思考中...";
        if(sendButton != null) sendButton.interactable = false;

        // 只封裝 contents，不加任何額外參數
        var requestData = new { contents = chatHistory };
        string jsonPayload = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest($"{url}?key={apiKey}", "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                var response = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);
                if (response.candidates != null && response.candidates.Count > 0) {
                    string aiReply = response.candidates[0].content.parts[0].text;
                    responseText.text = aiReply;
                    chatHistory.Add(new Content { role = "model", parts = new List<Part> { new Part { text = aiReply } } });
                }
            } else {
                // 如果還是報錯，我們把錯誤碼印出來
                Debug.LogError("API Error: " + request.downloadHandler.text);
                responseText.text = "連線失敗，請檢查 API Key 或網路。";
            }
        }
        if(sendButton != null) sendButton.interactable = true;
    }
}