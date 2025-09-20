using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class ChatMessage {
    public string role;
    public string content;
    public ChatMessage(string role, string content) {
        this.role = role;
        this.content = content;
    }
}

[System.Serializable]
public class ChatRequest {
    public string model;
    public ChatMessage[] messages;
    public int max_tokens;
}

[System.Serializable]
public class AIManager : MonoBehaviour
{

    public UIBehaviour uiBehaviour;

    private List<ChatMessage> chatHistory = new List<ChatMessage>();

    public void ResetChatHistory() => chatHistory.Clear();

    // Classes to parse server response safely
    [System.Serializable]
    public class AIChoice {
        public ChatMessage message;
    }

    [System.Serializable]
    public class AIResponse {
        public AIChoice[] choices;
        public AIError error; // optional
    }

    [System.Serializable]
    public class AIError {
        public string message;
        public string type;
        public string code;
    }

    public IEnumerator SendMessageToAI(string playerMessage, System.Action<string> callback)
    {
        if (chatHistory == null) chatHistory = new List<ChatMessage>();

        string systemPrompt = $"You are the AI within a video game with the theme of the player managing an AI company. Your AI level is {uiBehaviour.aiLevel}. The name of the company that developed you is {uiBehaviour.companyName} which currently has {uiBehaviour.employees} employees and has {uiBehaviour.infrastructure} data centers. Keep responses brief and on-topic. Assume an AI level of 1 outputs basic, vague answers with poor grammar, lack of capital letters/puncuation, and a lack of knowledge in general. While as the level gets higher, detailed and specific answers are expected, grammar and punctuation improve with each level assuming a max of level 10. You should cater your responses as if a customer of the company is asking you questions. Keep your response length within 50 tokens.";

        if (chatHistory.Count == 0)
            chatHistory.Add(new ChatMessage("system", systemPrompt));

        chatHistory.Add(new ChatMessage("user", playerMessage));

        ChatRequest req = new ChatRequest
        {
            model = "gpt-4o-mini",
            messages = chatHistory.ToArray(),
            max_tokens = 50
        };

        string jsonBody = JsonConvert.SerializeObject(req);

        using (UnityWebRequest www = new UnityWebRequest("http://localhost:3000/chat", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            string aiMessage = "No response.";

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network error: " + www.error);
                aiMessage = "Error contacting AI.";
            }
            else
            {
                string responseText = www.downloadHandler?.text;
                Debug.Log("Raw AI Response: " + responseText);

                try
                {
                    AIResponse aiResp = JsonConvert.DeserializeObject<AIResponse>(responseText);

                    // If server returned an error
                    if (aiResp?.error != null)
                    {
                        aiMessage = $"Server Error: {aiResp.error.message}";
                    }
                    // If server returned choices
                    else if (aiResp?.choices != null && aiResp.choices.Length > 0 && aiResp.choices[0]?.message != null)
                    {
                        aiMessage = aiResp.choices[0].message.content;
                        chatHistory.Add(new ChatMessage("assistant", aiMessage));
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parse Error: " + e);
                    aiMessage = "Error parsing AI response.";
                }
            }

            callback?.Invoke(aiMessage);
        }
    }
}
