using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HapticController : MonoBehaviour
{
    private Animator animator;
    private VestVisualization vestViz;
    private PoseReceiver poseReceiver;
    private AICoach aiCoach;
    private UdpClient hapticUdp;
    private bool goingDown = false;
    private bool goingUp = false;
    private bool wasInSync = true;
    private float repAccuracySum = 0f;
    private int repAccuracyFrames = 0;

    public float kneeAngleStanding = 170f;
    public float kneeAngleSquatting = 80f;
    public float syncTolerance = 100f;

    void Start()
    {
        animator = GetComponent<Animator>();
        vestViz = FindFirstObjectByType<VestVisualization>();
        poseReceiver = FindFirstObjectByType<PoseReceiver>();
        aiCoach = FindFirstObjectByType<AICoach>();
        hapticUdp = new UdpClient();
    }

    void SendHapticCommand(string jsonCommand)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(jsonCommand);
            hapticUdp.Send(data, data.Length, "127.0.0.1", 5067);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Haptic send error: " + e.Message);
        }
    }

    void Update()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = stateInfo.normalizedTime % 1f;

        float modelSquatProgress;
        if (normalizedTime < 0.5f)
            modelSquatProgress = normalizedTime / 0.5f;
        else
            modelSquatProgress = 1f - ((normalizedTime - 0.5f) / 0.5f);

        float expectedKneeAngle = Mathf.Lerp(kneeAngleStanding, kneeAngleSquatting, modelSquatProgress);

        bool inSync = true;
        float angleDifference = 0f;

        if (poseReceiver != null && poseReceiver.isReceiving)
        {
            float userKneeAngle = (poseReceiver.leftKneeAngle + poseReceiver.rightKneeAngle) / 2f;
            angleDifference = Mathf.Abs(userKneeAngle - expectedKneeAngle);
            inSync = angleDifference < syncTolerance;

            // Track rep accuracy
            float frameAccuracy = Mathf.Clamp01(1f - (angleDifference / syncTolerance)) * 100f;
            repAccuracySum += frameAccuracy;
            repAccuracyFrames++;

            // Notify AI Coach
            if (aiCoach != null)
            {
                if (inSync)
                    aiCoach.OnSyncFrame(angleDifference, syncTolerance);
                else
                    aiCoach.OnOutOfSyncFrame();
            }
        }

        if (vestViz != null)
        {
            if (inSync)
                vestViz.UpdateFromAnimation(stateInfo.normalizedTime);
            else
                vestViz.ShowOutOfSync();
        }

        if (inSync)
        {
            if (normalizedTime < 0.5f && !goingDown)
            {
                goingDown = true;
                goingUp = false;
                SendHapticCommand("{\"type\":\"event\",\"name\":\"squaddown\"}");
            }
            else if (normalizedTime >= 0.5f && !goingUp)
            {
                goingUp = true;
                goingDown = false;
                SendHapticCommand("{\"type\":\"event\",\"name\":\"squatup\"}");

                // Rep completed (finished going up)
                if (aiCoach != null && repAccuracyFrames > 0)
                {
                    float repAccuracy = repAccuracySum / repAccuracyFrames;
                    aiCoach.OnRepCompleted(repAccuracy);
                    repAccuracySum = 0f;
                    repAccuracyFrames = 0;
                }
            }
        }
        else
        {
            goingDown = false;
            goingUp = false;
            SendHapticCommand("{\"type\":\"alert\"}");
        }
    }

    void OnDestroy()
    {
        if (hapticUdp != null) hapticUdp.Close();
    }
}