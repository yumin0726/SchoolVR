using UnityEngine;
using System.Collections;

public class GeminiAudioHandler : MonoBehaviour {
    public AIManager aiManager;
    public TMPro.TMP_Text statusText;
    private AudioClip recording;
    private string micName;
    private bool isRecording = false;

    void Start() {
        if (Microphone.devices.Length > 0) {
            micName = Microphone.devices[0];
            Debug.Log("成功抓取麥克風: " + micName);
        } else {
            Debug.LogError("錯誤：找不到任何麥克風設備！");
            if (statusText != null) statusText.text = "未偵測到麥克風";
        }
    }

    // 當按鈕按下時執行
    public void StartRecording() {
        if (isRecording) return;
        Debug.Log(">>> 按鈕按下：錄音開始");
        
        isRecording = true;
        if (statusText != null) statusText.text = "🎤 錄音中 (限 8 秒)...";
        recording = Microphone.Start(micName, false, 8, 44100);
    }

    // 當按鈕放開時執行
    public void StopRecording() {
        if (!isRecording) return;
        Debug.Log(">>> 按鈕放開：錄音結束，準備傳送");

        isRecording = false;
        Microphone.End(micName);
        if (statusText != null) statusText.text = "⏳ 傳送給老皮...";
        
        // 轉換 WAV 並傳送
        byte[] audioByte = WavUtility.FromAudioClip(recording);
        if (aiManager != null) {
            aiManager.SendAudioToGemini(audioByte);
        }
    }
}