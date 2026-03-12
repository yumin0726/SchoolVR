using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Newtonsoft.Json;

public class AIManager : MonoBehaviour {
    [Header("API 設定")]
    [SerializeField] private string apiKey = "AIzaSyCiGbOXsjGRUIg2Hv9rrJB-lbE09y0zrO0"; 
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("NPC 設定")]
    [TextArea(3, 5)]
    public string npcPersonality = "你是一個幽默的寶藏獵人，名字叫老皮。";

    [Header("UI 引用")]
    public TMP_InputField inputField;
    public TMP_Text responseText;
    public UnityEngine.UI.Button sendButton;

    [Header("打字機設定")]
    public float typingSpeed = 0.05f;

    [Header("語音輸出設定")]
    public AudioSource voicePlayer; 

    private List<Content> chatHistory = new List<Content>();
    private Coroutine typewriterCoroutine;

    void Start() {
        // 初始化個性設定
        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = $"從現在起，你的個性設定如下：{npcPersonality}。請以此身分開始對話。" } }
        });
    
        chatHistory.Add(new Content {
            role = "model",
            parts = new List<Part> { new Part { text = "沒問題，我已經準備好以此身分與你交流了！請問有什麼事嗎？" } }
        });
        Debug.Log("老皮的大腦已就緒。");
    }

    // --- 文字發送 ---
    public void OnSendClick() {
        if (string.IsNullOrEmpty(inputField.text)) return;
        Debug.Log("發送文字訊息: " + inputField.text);

        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = inputField.text } }
        });

        StartCoroutine(PostToGemini());
        inputField.text = "";
    }

    IEnumerator PostToGemini() {
        responseText.text = "老皮正在思考中...";
        if(sendButton != null) sendButton.interactable = false;

        var requestData = new { contents = chatHistory };
        string jsonPayload = JsonConvert.SerializeObject(requestData);

        yield return SendRequest(jsonPayload);
        if(sendButton != null) sendButton.interactable = true;
    }

    // --- 語音發送 (由 GeminiAudioHandler 呼叫) ---
    public void SendAudioToGemini(byte[] audioData) {
        string base64Audio = System.Convert.ToBase64String(audioData);
        StartCoroutine(PostAudioToGemini(base64Audio));
    }

    IEnumerator PostAudioToGemini(string base64Audio) {
        Debug.Log("正在將語音上傳至 Gemini...");
        responseText.text = "老皮正在聽你說話...";
        if(sendButton != null) sendButton.interactable = false;

        var requestData = new {
            contents = new[] {
                new {
                    role = "user",
                    parts = new object[] {
                        new { text = $"你是{npcPersonality}，請聽這段語音並以你的身分跟我對話，不要解釋語音內容，直接回答我：" }, 
                        new { inline_data = new { mime_type = "audio/wav", data = base64Audio } }
                    }
                }
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestData);
        yield return SendRequest(jsonPayload);
        if(sendButton != null) sendButton.interactable = true;
    }

    // --- 網路請求核心 ---
    IEnumerator SendRequest(string jsonPayload) {
        using (UnityWebRequest request = new UnityWebRequest($"{url}?key={apiKey}", "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // --- 新增這行偵錯 ---
            Debug.Log("API 原始回傳內容: " + request.downloadHandler.text); 

            if (request.result == UnityWebRequest.Result.Success) {
                var response = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);
            
                if (response != null && response.candidates != null && response.candidates.Count > 0) {
                    string aiReply = response.candidates[0].content.parts[0].text;
                    Debug.Log("老皮說了: " + aiReply); // 確認 AI 有沒有說話

                    if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                    typewriterCoroutine = StartCoroutine(TypeText(aiReply));

                    chatHistory.Add(new Content { role = "model", parts = new List<Part> { new Part { text = aiReply } } });
                } else {
                    Debug.LogWarning("收到回覆，但找不到內容 (可能被過濾了)");
                }
            } else {
                Debug.LogError("Gemini API 錯誤: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator TypeText(string text) {
        StartCoroutine(DownloadAndPlayVoice(text));
        responseText.text = "";
        foreach (char letter in text.ToCharArray()) {
            responseText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator DownloadAndPlayVoice(string text) {
        string shortText = text.Length > 100 ? text.Substring(0, 100) : text;
        string ttsUrl = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q=" 
                    + UnityWebRequest.EscapeURL(shortText) + "&tl=zh-TW";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(ttsUrl, AudioType.MPEG)) {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (voicePlayer != null) {
                    voicePlayer.clip = clip;
                    voicePlayer.Play();
                }
            }
        }
    }

    // 這些類別要放在 AIManager 類別裡面，或是下方
    [System.Serializable]
    public class GeminiResponse { public List<Candidate> candidates; }
    [System.Serializable]
    public class Candidate { public Content content; }
    [System.Serializable]
    public class Content { public string role; public List<Part> parts; }
    [System.Serializable]
    public class Part { public string text; }
}