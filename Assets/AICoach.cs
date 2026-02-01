using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AICoach : MonoBehaviour
{
    private Text coachText;
    private float messageTimer = 0f;
    private float messageDuration = 2f;
    private int lastRepMilestone = 0;
    private int consecutiveSyncFrames = 0;
    private int consecutiveOutOfSyncFrames = 0;
    private bool isActive = false;

    // Performance tracking
    private int totalReps = 0;
    private float totalAccuracy = 0f;
    private int perfectReps = 0;
    private int bestStreak = 0;
    private int currentStreak = 0;
    private List<string> sessionNotes = new List<string>();

    // Message pools
    private string[] syncMessages = {
        "Great form!",
        "You're nailing it!",
        "Perfect rhythm!",
        "Keep it up!",
        "Excellent pace!",
        "Looking strong!",
        "That's the way!",
        "Smooth movement!",
        "You're in the zone!",
        "Beautiful form!"
    };

    private string[] outOfSyncMessages = {
        "Try to match the model!",
        "Slow down a little!",
        "Watch the model's pace!",
        "Almost there, adjust your timing!",
        "Focus on the rhythm!",
        "Follow the model's lead!",
        "You can do it, stay with it!",
        "Take a breath and reset!"
    };

    private string[] repMilestoneMessages = {
        "reps! Nice work!",
        "reps! Keep going!",
        "reps! You're crushing it!",
        "reps! Awesome job!",
        "reps! Stay strong!"
    };

    private string[] streakMessages = {
        "3 perfect reps in a row!",
        "5 rep streak! On fire!",
        "10 rep streak! Unstoppable!",
        "15 rep streak! Incredible!",
        "20 rep streak! Champion!"
    };

    private string[] perfectMessages = {
        "PERFECT!",
        "Flawless!",
        "Spot on!",
        "Bullseye!",
        "Nailed it!"
    };

    void Start()
    {
        CreateCoachUI();
    }

    void CreateCoachUI()
    {
        GameObject obj = new GameObject("CoachText");
        obj.transform.SetParent(transform, false);
        coachText = obj.AddComponent<Text>();
        coachText.text = "";
        coachText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        coachText.fontSize = 26;
        coachText.color = Color.white;
        coachText.alignment = TextAnchor.MiddleCenter;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.2f, 0.65f);
        rect.anchorMax = new Vector2(0.8f, 0.75f);
        rect.sizeDelta = Vector2.zero;
    }

    public void Activate()
    {
        isActive = true;
        totalReps = 0;
        totalAccuracy = 0f;
        perfectReps = 0;
        bestStreak = 0;
        currentStreak = 0;
        lastRepMilestone = 0;
        consecutiveSyncFrames = 0;
        consecutiveOutOfSyncFrames = 0;
        sessionNotes.Clear();
        ShowMessage("Let's go! Follow the model!", Color.white);
    }

    public void Deactivate()
    {
        isActive = false;
        coachText.text = "";
    }

    void Update()
    {
        if (!isActive) return;

        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                coachText.text = "";
            }
        }
    }

    public void OnSyncFrame(float angleDifference, float tolerance)
    {
        if (!isActive) return;

        consecutiveSyncFrames++;
        consecutiveOutOfSyncFrames = 0;

        if (angleDifference < tolerance * 0.3f)
        {
            // Very close to perfect
            if (consecutiveSyncFrames % 60 == 0 && messageTimer <= 0)
            {
                ShowMessage(perfectMessages[Random.Range(0, perfectMessages.Length)],
                    new Color(0f, 1f, 0.5f));
            }
        }
        else if (consecutiveSyncFrames % 120 == 0 && messageTimer <= 0)
        {
            ShowMessage(syncMessages[Random.Range(0, syncMessages.Length)],
                new Color(0.5f, 1f, 0.5f));
        }
    }

    public void OnOutOfSyncFrame()
    {
        if (!isActive) return;

        consecutiveOutOfSyncFrames++;
        consecutiveSyncFrames = 0;

        if (currentStreak > 0)
        {
            if (currentStreak > bestStreak) bestStreak = currentStreak;
            currentStreak = 0;
        }

        if (consecutiveOutOfSyncFrames == 30 && messageTimer <= 0)
        {
            ShowMessage(outOfSyncMessages[Random.Range(0, outOfSyncMessages.Length)],
                new Color(1f, 0.8f, 0.3f));
        }
    }

    public void OnRepCompleted(float repAccuracy)
    {
        if (!isActive) return;

        totalReps++;
        totalAccuracy += repAccuracy;

        if (repAccuracy > 80f)
        {
            currentStreak++;
            if (repAccuracy > 95f) perfectReps++;
        }
        else
        {
            if (currentStreak > bestStreak) bestStreak = currentStreak;
            currentStreak = 0;
        }

        // Check streak milestones
        if (currentStreak == 3)
            ShowMessage(streakMessages[0], new Color(1f, 0.8f, 0f));
        else if (currentStreak == 5)
            ShowMessage(streakMessages[1], new Color(1f, 0.8f, 0f));
        else if (currentStreak == 10)
            ShowMessage(streakMessages[2], new Color(1f, 0.5f, 0f));
        else if (currentStreak == 15)
            ShowMessage(streakMessages[3], new Color(1f, 0.3f, 0f));
        else if (currentStreak == 20)
            ShowMessage(streakMessages[4], new Color(1f, 0f, 0f));

        // Check rep milestones (every 5 reps)
        int milestone = (totalReps / 5) * 5;
        if (milestone > 0 && milestone > lastRepMilestone)
        {
            lastRepMilestone = milestone;
            string msg = milestone + " " +
                repMilestoneMessages[Random.Range(0, repMilestoneMessages.Length)];
            ShowMessage(msg, new Color(0f, 0.8f, 1f));
        }
    }

    public string GetSessionSummary(float overallAccuracy, int reps, bool isLenient)
    {
        string summary = "";

        // Star rating
        int stars = 1;
        if (overallAccuracy >= 60f) stars = 2;
        if (overallAccuracy >= 80f) stars = 3;

        string starDisplay = "";
        for (int i = 0; i < stars; i++) starDisplay += "* ";

        summary += starDisplay + "\n\n";

        // Performance comment
        if (overallAccuracy >= 90f)
            summary += "Outstanding performance!\n";
        else if (overallAccuracy >= 75f)
            summary += "Great workout! Really solid form.\n";
        else if (overallAccuracy >= 60f)
            summary += "Good effort! You're making progress.\n";
        else if (overallAccuracy >= 40f)
            summary += "Nice try! Keep practicing and you'll improve.\n";
        else
            summary += "Good start! It takes time to build the rhythm.\n";

        // Stats
        summary += "\nTotal Reps: " + reps;
        summary += "\nAccuracy: " + overallAccuracy.ToString("F1") + "%";
        summary += "\nPerfect Reps: " + perfectReps;
        summary += "\nBest Streak: " + bestStreak;
        summary += "\nMode: " + (isLenient ? "Lenient" : "Strict");

        // Recommendation
        summary += "\n\n";
        if (isLenient && overallAccuracy >= 80f)
            summary += "Coach Tip: You're doing amazing on Lenient!\nReady to try Strict mode?";
        else if (!isLenient && overallAccuracy < 40f)
            summary += "Coach Tip: Try Lenient mode to build\nconfidence, then come back to Strict!";
        else if (overallAccuracy >= 70f)
            summary += "Coach Tip: Focus on going deeper in\nyour squats for even better results!";
        else
            summary += "Coach Tip: Watch the model closely and\ntry to match its rhythm. You've got this!";

        return summary;
    }

    void ShowMessage(string message, Color color)
    {
        coachText.text = message;
        coachText.color = color;
        messageTimer = messageDuration;
    }
}