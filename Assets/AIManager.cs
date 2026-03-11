using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Newtonsoft.Json;

public class AIManager : MonoBehaviour {
    [Header("API 設定")]
    [SerializeField] private string apiKey = "AIzaSyDiBuI-fGSiaG1945hku8_OCsdZ-w-rVbM"; // 建議之後用設定檔保存
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

    [Header("語音設定")]
    public AudioSource voicePlayer; 

    private List<Content> chatHistory = new List<Content>();
    private Coroutine typewriterCoroutine;

    void Start() {
        // 設定初始個性
        chatHistory.Add(new Content {
            role = "user",
            parts = new List<Part> { new Part { text = $"從現在起，你的個性設定如下：{npcPersonality}。請以此身分開始對話。" } }
        });
    
        chatHistory.Add(new Content {
            role = "model",
            parts = new List<Part> { new Part { text = "沒問題，我已經準備好以此身分與你交流了！請問有什麼事嗎？" } }
        });
    }

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
        responseText.text = "老皮正在思考中...";
        if(sendButton != null) sendButton.interactable = false;

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
                    
                    // 關鍵修改：這裡改為呼叫 TypeText，才會啟動打字機和語音
                    if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                    typewriterCoroutine = StartCoroutine(TypeText(aiReply));

                    chatHistory.Add(new Content { role = "model", parts = new List<Part> { new Part { text = aiReply } } });
                }
            } else {
                Debug.LogError("API Error: " + request.downloadHandler.text);
                responseText.text = "連線失敗，請檢查網路。";
            }
        }
        if(sendButton != null) sendButton.interactable = true;
    }

    IEnumerator TypeText(string text) {
        // 先觸發語音下載與播放
        StartCoroutine(DownloadAndPlayVoice(text));

        responseText.text = "";
        foreach (char letter in text.ToCharArray()) {
            responseText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator DownloadAndPlayVoice(string text) {
        // 限制語音長度（Google TTS 免費版限制）
        string shortText = text.Length > 100 ? text.Substring(0, 100) : text;
    
        string ttsUrl = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q=" 
                    + UnityWebRequest.EscapeURL(shortText) + "&tl=zh-TW";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(ttsUrl, AudioType.MPEG)) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) {
                Debug.Log("語音下載成功，開始播放！");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (voicePlayer != null) {
                    voicePlayer.clip = clip;
                    voicePlayer.Play();
                }
            } else {
                Debug.LogError("語音下載失敗：" + www.error);
            }
        }
    }
}