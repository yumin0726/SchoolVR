using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Newtonsoft.Json;

public class AIManager : MonoBehaviour {
    [Header("API 設定")]
    [SerializeField] private string apiKey = "AIzaSyD9kxEzG2LX1W2KbTp3PANpuykW3KaDZHQ"; 
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("NPC 設定")]
    [TextArea(3, 5)]
    public string npcPersonality = "你是一個幽默的寶藏獵人，名字叫老皮。";

    [Header("校園知識庫 (手動輸入)")]
    [TextArea(10, 20)]
    public string schoolKnowledge = "在此輸入關於宜大的詳細資料，老皮會讀取這些內容來回答...";

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
        // 初始化時就把「個性」與「知識庫」餵給 AI
        string initialSystemPrompt = $"你的角色設定：{npcPersonality}\n\n你的校園知識庫如下：\n{schoolKnowledge}\n\n請以你的身分並根據知識庫回答玩家。";
        
        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = initialSystemPrompt } }
        });
    
        chatHistory.Add(new Content {
            role = "model",
            parts = new List<Part> { new Part { text = "沒問題，我已經掌握了這些校園知識，我是老皮，準備好為你指路了！" } }
        });
        Debug.Log("老皮的大腦已就緒，手動知識庫已載入。");
    }

    // --- 文字發送 ---
    public void OnSendClick() {
        if (string.IsNullOrEmpty(inputField.text)) return;
        
        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = inputField.text } }
        });

        StartCoroutine(PostToGemini());
        inputField.text = "";
    }

    IEnumerator PostToGemini() {
        responseText.text = "老皮正在翻閱大腦紀錄...";
        if(sendButton != null) sendButton.interactable = false;

        var requestData = new { contents = chatHistory };
        string jsonPayload = JsonConvert.SerializeObject(requestData);

        yield return SendRequest(jsonPayload);
        if(sendButton != null) sendButton.interactable = true;
    }

    // --- 語音發送 ---
    public void SendAudioToGemini(byte[] audioData) {
        string base64Audio = System.Convert.ToBase64String(audioData);
        StartCoroutine(PostAudioToGemini(base64Audio));
    }

    IEnumerator PostAudioToGemini(string base64Audio) {
        responseText.text = "老皮正在聽你說話...";
        if(sendButton != null) sendButton.interactable = false;

        // 語音請求：在指令中再次強調知識庫，確保精準度
        var requestData = new {
            contents = new[] {
                new {
                    role = "user",
                    parts = new object[] {
                        new { text = $"根據以下知識庫內容回答玩家語音：\n{schoolKnowledge}\n\n玩家語音內容：" }, 
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

            if (request.result == UnityWebRequest.Result.Success) {
                var response = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);
            
                if (response != null && response.candidates != null && response.candidates.Count > 0) {
                    string aiReply = response.candidates[0].content.parts[0].text;
                    
                    if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                    typewriterCoroutine = StartCoroutine(TypeText(aiReply));

                    chatHistory.Add(new Content { role = "model", parts = new List<Part> { new Part { text = aiReply } } });
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

    [System.Serializable] public class GeminiResponse { public List<Candidate> candidates; }
    [System.Serializable] public class Candidate { public Content content; }
    [System.Serializable] public class Content { public string role; public List<Part> parts; }
    [System.Serializable] public class Part { public string text; }
}