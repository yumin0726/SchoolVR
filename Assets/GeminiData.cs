using System;
using System.Collections.Generic;

// 1. 請求結構
[Serializable]
public class GeminiRequest {
    public List<Content> contents;
    //public SystemInstruction system_instruction; 
}

// 2. 系統指令 (設定 NPC 個性用)
[Serializable]
public class SystemInstruction {
    public List<Part> parts;
}

// 3. 對話內容
[Serializable]
public class Content {
    public string role; // "user" 或 "model"
    public List<Part> parts;
}

// 4. 文字單位
[Serializable]
public class Part {
    public string text;
}

// --- 解析回傳結果用的類別 ---
[Serializable]
public class GeminiResponse {
    public List<Candidate> candidates;
}

[Serializable]
public class Candidate {
    public Content content;
}