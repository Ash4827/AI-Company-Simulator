using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField playerInput;     // Input field for player
    public ScrollRect scrollRect;          // Scroll View
    public RectTransform contentPanel;     // Content panel
    public TMP_Text messagePrefab;         // Prefab for messages
    public AIManager aiManager;            // Reference to AIManager

    public void OnSendButton()
    {
        string message = playerInput.text;
        if (string.IsNullOrEmpty(message)) return;

        AddMessage("Player: " + message);

        StartCoroutine(aiManager.SendMessageToAI(message, (response) =>
        {
            AddMessage("AI: " + response);
        }));

        playerInput.text = "";
    }

    private void AddMessage(string text)
    {
        TMP_Text newMsg = Instantiate(messagePrefab, contentPanel);
        newMsg.text = text;
        newMsg.gameObject.SetActive(true);

        // Update layout and scroll to bottom
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnResetChat()
    {
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        aiManager.ResetChatHistory();
    }
}
