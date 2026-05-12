using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OllamaAI : MonoBehaviour
{
    private string ollamaUrl = "http://localhost:11434/api/generate";
    private string model = "qwen3.6:latest";

    private GameObject chatPanel;
    private Text responseText;
    private InputField inputField;
    private Button sendButton;
    private Button toggleButton;
    private bool chatVisible = false;
    private ScrollRect scrollRect;

    private string systemPrompt = @"You are FunFit AI, a friendly and encouraging fitness coach inside a haptic fitness game. 
The game tracks users doing squats with a webcam and gives haptic feedback through a bHaptics vest.
There are two modes: Strict (for advanced users) and Lenient (for users with motor difficulties, designed for GiGi's Playhouse participants with Down syndrome).
Keep responses short (2-3 sentences max) and encouraging. Use simple language.
When giving workout plans, format them clearly with exercise names, reps, and rest times.
You care about accessibility and making fitness fun for everyone.";

    void Start()
    {
        CreateChatUI();
        CreateToggleButton();
        chatPanel.SetActive(false);
    }

    void CreateToggleButton()
    {
        GameObject btnObj = new GameObject("AIToggle");
        btnObj.transform.SetParent(transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0);
        btnRect.anchorMax = new Vector2(0, 0);
        btnRect.pivot = new Vector2(0, 0);
        btnRect.anchoredPosition = new Vector2(10, 10);
        btnRect.sizeDelta = new Vector2(120, 40);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.8f, 0.9f);

        toggleButton = btnObj.AddComponent<Button>();
        toggleButton.onClick.AddListener(ToggleChat);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text btnText = textObj.AddComponent<Text>();
        btnText.text = "AI Coach";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 18;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    void CreateChatUI()
    {
        chatPanel = new GameObject("ChatPanel");
        chatPanel.transform.SetParent(transform, false);
        RectTransform panelRect = chatPanel.AddComponent<RectTransform>();
        Image panelImage = chatPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelRect.anchorMin = new Vector2(0, 0.05f);
        panelRect.anchorMax = new Vector2(0.35f, 0.55f);
        panelRect.pivot = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(10, 60);
        panelRect.sizeDelta = Vector2.zero;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(chatPanel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "FunFit AI Coach";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 20;
        titleText.color = new Color(0f, 1f, 0.5f);
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = Vector2.zero;

        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(chatPanel.transform, false);
        RectTransform scrollRectTransform = scrollObj.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.02f, 0.15f);
        scrollRectTransform.anchorMax = new Vector2(0.98f, 0.88f);
        scrollRectTransform.sizeDelta = Vector2.zero;

        scrollRect = scrollObj.AddComponent<ScrollRect>();
        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.1f, 1f);

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.viewport = scrollRectTransform;

        responseText = contentObj.AddComponent<Text>();
        responseText.text = "Hi! I'm your FunFit AI Coach. Ask me anything about your workout, or I can create a custom plan for you!";
        responseText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        responseText.fontSize = 16;
        responseText.color = Color.white;
        responseText.alignment = TextAnchor.UpperLeft;

        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(chatPanel.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.02f, 0.02f);
        inputRect.anchorMax = new Vector2(0.78f, 0.13f);
        inputRect.sizeDelta = Vector2.zero;

        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        inputField = inputObj.AddComponent<InputField>();

        GameObject inputTextObj = new GameObject("Text");
        inputTextObj.transform.SetParent(inputObj.transform, false);
        Text inputText = inputTextObj.AddComponent<Text>();
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize = 14;
        inputText.color = Color.white;
        inputText.alignment = TextAnchor.MiddleLeft;
        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = new Vector2(0.05f, 0);
        inputTextRect.anchorMax = new Vector2(0.95f, 1);
        inputTextRect.sizeDelta = Vector2.zero;

        inputField.textComponent = inputText;

        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        Text placeholder = placeholderObj.AddComponent<Text>();
        placeholder.text = "Ask me anything...";
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = 14;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f);
        placeholder.alignment = TextAnchor.MiddleLeft;
        placeholder.fontStyle = FontStyle.Italic;
        RectTransform phRect = placeholderObj.GetComponent<RectTransform>();
        phRect.anchorMin = new Vector2(0.05f, 0);
        phRect.anchorMax = new Vector2(0.95f, 1);
        phRect.sizeDelta = Vector2.zero;

        inputField.placeholder = placeholder;

        GameObject sendObj = new GameObject("SendBtn");
        sendObj.transform.SetParent(chatPanel.transform, false);
        RectTransform sendRect = sendObj.AddComponent<RectTransform>();
        sendRect.anchorMin = new Vector2(0.8f, 0.02f);
        sendRect.anchorMax = new Vector2(0.98f, 0.13f);
        sendRect.sizeDelta = Vector2.zero;

        Image sendBg = sendObj.AddComponent<Image>();
        sendBg.color = new Color(0f, 0.7f, 0.3f, 1f);

        sendButton = sendObj.AddComponent<Button>();
        sendButton.onClick.AddListener(OnSendClicked);

        GameObject sendTextObj = new GameObject("Text");
        sendTextObj.transform.SetParent(sendObj.transform, false);
        Text sendText = sendTextObj.AddComponent<Text>();
        sendText.text = "Send";
        sendText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sendText.fontSize = 16;
        sendText.color = Color.white;
        sendText.alignment = TextAnchor.MiddleCenter;
        RectTransform sendTextRect = sendTextObj.GetComponent<RectTransform>();
        sendTextRect.anchorMin = Vector2.zero;
        sendTextRect.anchorMax = Vector2.one;
        sendTextRect.sizeDelta = Vector2.zero;
    }

    void ToggleChat()
    {
        chatVisible = !chatVisible;
        chatPanel.SetActive(chatVisible);
    }

    void OnSendClicked()
    {
        string userMessage = inputField.text;
        if (string.IsNullOrEmpty(userMessage)) return;

        responseText.text += "\n\nYou: " + userMessage;
        inputField.text = "";

        StartCoroutine(SendToOllama(userMessage));
    }

    public void SendPerformanceData(int reps, float accuracy, bool isLenient, int bestStreak)
    {
        string prompt = "The user just finished a workout session. Here are their stats: " +
            "Reps: " + reps + ", Accuracy: " + accuracy.ToString("F1") + "%, " +
            "Mode: " + (isLenient ? "Lenient" : "Strict") + ", " +
            "Best Streak: " + bestStreak + ". " +
            "Give them a brief encouraging summary and one tip to improve.";

        StartCoroutine(SendToOllama(prompt));
    }

    public void RequestWorkoutPlan(bool isLenient)
    {
        string prompt = "Create a short 5-minute workout plan for someone using the " +
            (isLenient ? "Lenient" : "Strict") + " mode. " +
            (isLenient ? "This person may have motor difficulties, so keep exercises simple and accessible. " : "") +
            "Include exercise names, number of reps, and rest times. Keep it fun and encouraging.";

        StartCoroutine(SendToOllama(prompt));
    }

    IEnumerator SendToOllama(string prompt)
    {
        responseText.text += "\n\nAI Coach: Thinking...";

        string jsonBody = "{\"model\":\"" + model + "\",\"prompt\":\"" +
            EscapeJson(systemPrompt + "\\n\\nUser: " + prompt) +
            "\",\"stream\":false}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 60;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;

            string aiResponse = "";
            try
            {
                int fieldStart = response.IndexOf("\"response\":\"");
                if (fieldStart >= 0)
                {
                    fieldStart += 12;
                    StringBuilder sb = new StringBuilder();
                    bool escaped = false;
                    for (int i = fieldStart; i < response.Length; i++)
                    {
                        char c = response[i];
                        if (escaped)
                        {
                            if (c == 'n') sb.Append('\n');
                            else if (c == 't') sb.Append('\t');
                            else if (c == '"') sb.Append('"');
                            else if (c == '\\') sb.Append('\\');
                            else sb.Append(c);
                            escaped = false;
                        }
                        else if (c == '\\')
                        {
                            escaped = true;
                        }
                        else if (c == '"')
                        {
                            break;
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    aiResponse = sb.ToString();
                }
            }
            catch
            {
                aiResponse = "Sorry, I had trouble processing that. Try again!";
            }

            responseText.text = responseText.text.Replace("Thinking...", aiResponse);
        }
        else
        {
            responseText.text = responseText.text.Replace("Thinking...",
                "Sorry, I couldn't connect. Make sure Ollama is running!");
            Debug.LogError("Ollama error: " + request.error);
        }
    }

    string EscapeJson(string input)
    {
        return input.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
    }
}