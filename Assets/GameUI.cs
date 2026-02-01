using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    // Rep counter
    private Text repText;
    private int repCount = 0;
    private bool wasSquatting = false;

    // Sync feedback
    private Text feedbackText;
    private float feedbackTimer = 0f;

    // Workout timer
    private Text timerText;
    private float workoutTime = 60f;
    private float timeRemaining;
    private bool workoutActive = false;

    // Difficulty
    private Text difficultyText;
    private bool isLenient = false;

    // Results screen
    private GameObject resultsPanel;
    private Text resultsText;
    private int totalFrames = 0;
    private int syncFrames = 0;

    // Start menu
    private GameObject startPanel;
    private bool gameStarted = false;

    // References
    private HapticController hapticController;
    private PoseReceiver poseReceiver;

    void Start()
    {
        hapticController = FindFirstObjectByType<HapticController>();
        poseReceiver = FindFirstObjectByType<PoseReceiver>();
        timeRemaining = workoutTime;

        CreateStartMenu();
        CreateRepCounter();
        CreateFeedbackText();
        CreateTimer();
        CreateDifficultyLabel();
        CreateResultsScreen();

        // Hide game UI until started
        repText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        difficultyText.gameObject.SetActive(false);
    }

    void CreateStartMenu()
    {
        startPanel = new GameObject("StartPanel");
        startPanel.transform.SetParent(transform, false);
        RectTransform panelRect = startPanel.AddComponent<RectTransform>();
        Image panelImage = startPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(startPanel.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "HAPTIC FITNESS";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 48;
        title.color = new Color(0f, 1f, 0.5f);
        title.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.85f);
        titleRect.sizeDelta = Vector2.zero;

        // Strict button
        CreateButton(startPanel, "Strict Mode", new Vector2(0, 0.5f), () => StartGame(false));

        // Lenient button
        CreateButton(startPanel, "Lenient Mode", new Vector2(0, 0.35f), () => StartGame(true));
    }

    void CreateButton(GameObject parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(label + "Btn");
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.3f, position.y);
        btnRect.anchorMax = new Vector2(0.7f, position.y + 0.1f);
        btnRect.sizeDelta = Vector2.zero;

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text btnText = textObj.AddComponent<Text>();
        btnText.text = label;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 28;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    void StartGame(bool lenient)
    {
        isLenient = lenient;
        gameStarted = true;
        workoutActive = true;
        timeRemaining = workoutTime;
        repCount = 0;
        totalFrames = 0;
        syncFrames = 0;

        if (hapticController != null)
        {
            hapticController.syncTolerance = lenient ? 100f : 40f;
        }

        startPanel.SetActive(false);
        repText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        difficultyText.gameObject.SetActive(true);
        resultsPanel.SetActive(false);

        difficultyText.text = lenient ? "MODE: LENIENT" : "MODE: STRICT";
        difficultyText.color = lenient ? new Color(0.3f, 0.8f, 1f) : new Color(1f, 0.5f, 0.3f);

        AICoach coach = FindFirstObjectByType<AICoach>();
        if (coach != null) coach.Activate();
    }

    void CreateRepCounter()
    {
        GameObject obj = new GameObject("RepCounter");
        obj.transform.SetParent(transform, false);
        repText = obj.AddComponent<Text>();
        repText.text = "REPS: 0";
        repText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        repText.fontSize = 36;
        repText.color = Color.white;
        repText.alignment = TextAnchor.MiddleCenter;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.35f, 0.9f);
        rect.anchorMax = new Vector2(0.65f, 1f);
        rect.sizeDelta = Vector2.zero;
    }

    void CreateFeedbackText()
    {
        GameObject obj = new GameObject("Feedback");
        obj.transform.SetParent(transform, false);
        feedbackText = obj.AddComponent<Text>();
        feedbackText.text = "";
        feedbackText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        feedbackText.fontSize = 32;
        feedbackText.color = Color.green;
        feedbackText.alignment = TextAnchor.MiddleCenter;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.3f, 0.8f);
        rect.anchorMax = new Vector2(0.7f, 0.9f);
        rect.sizeDelta = Vector2.zero;
    }

    void CreateTimer()
    {
        GameObject obj = new GameObject("Timer");
        obj.transform.SetParent(transform, false);
        timerText = obj.AddComponent<Text>();
        timerText.text = "1:00";
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 28;
        timerText.color = Color.white;
        timerText.alignment = TextAnchor.MiddleRight;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.75f, 0.92f);
        rect.anchorMax = new Vector2(0.98f, 1f);
        rect.sizeDelta = Vector2.zero;
    }

    void CreateDifficultyLabel()
    {
        GameObject obj = new GameObject("Difficulty");
        obj.transform.SetParent(transform, false);
        difficultyText = obj.AddComponent<Text>();
        difficultyText.text = "MODE: STRICT";
        difficultyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        difficultyText.fontSize = 20;
        difficultyText.color = Color.white;
        difficultyText.alignment = TextAnchor.MiddleLeft;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.92f);
        rect.anchorMax = new Vector2(0.3f, 1f);
        rect.sizeDelta = Vector2.zero;
    }

    void CreateResultsScreen()
    {
        resultsPanel = new GameObject("ResultsPanel");
        resultsPanel.transform.SetParent(transform, false);
        RectTransform panelRect = resultsPanel.AddComponent<RectTransform>();
        Image panelImage = resultsPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("ResultsText");
        textObj.transform.SetParent(resultsPanel.transform, false);
        resultsText = textObj.AddComponent<Text>();
        resultsText.text = "";
        resultsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultsText.fontSize = 36;
        resultsText.color = Color.white;
        resultsText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.2f);
        textRect.anchorMax = new Vector2(0.9f, 0.8f);
        textRect.sizeDelta = Vector2.zero;

        CreateButton(resultsPanel, "Play Again", new Vector2(0, 0.1f), () => {
            resultsPanel.SetActive(false);
            startPanel.SetActive(true);
        });

        resultsPanel.SetActive(false);
    }

    void Update()
    {
        if (!gameStarted || !workoutActive) return;

        // Update timer
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            workoutActive = false;
            ShowResults();
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = minutes + ":" + seconds.ToString("00");

        if (timeRemaining < 10f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;

        // Track sync and reps
        if (poseReceiver != null && poseReceiver.isReceiving)
        {
            float userKneeAngle = (poseReceiver.leftKneeAngle + poseReceiver.rightKneeAngle) / 2f;
            bool isSquatting = userKneeAngle < 120f;

            // Count reps (stood up after squatting)
            if (wasSquatting && !isSquatting)
            {
                repCount++;
                repText.text = "REPS: " + repCount;
                ShowFeedback("Great rep!", Color.green);
            }
            wasSquatting = isSquatting;

            // Track accuracy
            totalFrames++;
            if (hapticController != null)
            {
                Animator anim = hapticController.GetComponent<Animator>();
                if (anim != null)
                {
                    AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
                    float normalizedTime = state.normalizedTime % 1f;
                    float modelProgress;
                    if (normalizedTime < 0.5f)
                        modelProgress = normalizedTime / 0.5f;
                    else
                        modelProgress = 1f - ((normalizedTime - 0.5f) / 0.5f);

                    float expectedAngle = Mathf.Lerp(170f, 80f, modelProgress);
                    float diff = Mathf.Abs(userKneeAngle - expectedAngle);

                    if (diff < hapticController.syncTolerance)
                    {
                        syncFrames++;
                        if (diff < 20f)
                            ShowFeedback("Perfect!", new Color(0f, 1f, 0.5f));
                        else if (diff < hapticController.syncTolerance * 0.5f)
                            ShowFeedback("Keep going!", Color.yellow);
                    }
                    else
                    {
                        ShowFeedback("Get back in sync!", Color.red);
                    }
                }
            }
        }

        // Fade feedback text
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
                feedbackText.text = "";
        }
    }

    void ShowFeedback(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackTimer = 1.5f;
    }

   void ShowResults()
{
    AICoach coach = FindFirstObjectByType<AICoach>();
    float accuracy = totalFrames > 0 ? (float)syncFrames / totalFrames * 100f : 0f;

    if (coach != null)
    {
        resultsText.text = coach.GetSessionSummary(accuracy, repCount, isLenient);
        coach.Deactivate();
    }
    else
    {
        resultsText.text = "WORKOUT COMPLETE!\n\n" +
            "Total Reps: " + repCount + "\n" +
            "Accuracy: " + accuracy.ToString("F1") + "%\n" +
            "Mode: " + (isLenient ? "Lenient" : "Strict");
    }

    repText.gameObject.SetActive(false);
    feedbackText.gameObject.SetActive(false);
    timerText.gameObject.SetActive(false);
    difficultyText.gameObject.SetActive(false);
    resultsPanel.SetActive(true);
}
}