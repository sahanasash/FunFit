using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PoseReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private string latestData = "";
    private bool isRunning = true;
    private bool newDataReceived = false;

    public float leftKneeAngle { get; private set; }
    public float rightKneeAngle { get; private set; }
    public float hipHeight { get; private set; }
    public bool isReceiving { get; private set; }

    void Start()
    {
        try
        {
            udpClient = new UdpClient(5065);
            Debug.Log("PoseReceiver: Listening on port 5065");
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("PoseReceiver failed to start: " + e.Message);
        }
    }

    void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref endPoint);
                latestData = Encoding.UTF8.GetString(data);
                newDataReceived = true;
            }
            catch (Exception e)
            {
                if (isRunning)
                    Debug.LogError("UDP Error: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (newDataReceived)
        {
            newDataReceived = false;
            Debug.Log("Received pose data!");

            try
            {
                ProcessLandmarks(latestData);
                isReceiving = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Process error: " + e.Message);
                isReceiving = false;
            }
        }
    }

    void ProcessLandmarks(string json)
    {
        try
        {
            Vector3 leftHip = ExtractLandmark(json, "23");
            Vector3 leftKnee = ExtractLandmark(json, "25");
            Vector3 leftAnkle = ExtractLandmark(json, "27");

            Vector3 rightHip = ExtractLandmark(json, "24");
            Vector3 rightKnee = ExtractLandmark(json, "26");
            Vector3 rightAnkle = ExtractLandmark(json, "28");

            leftKneeAngle = CalculateAngle(leftHip, leftKnee, leftAnkle);
            rightKneeAngle = CalculateAngle(rightHip, rightKnee, rightAnkle);
            hipHeight = (leftHip.y + rightHip.y) / 2f;

            Debug.Log("Left knee: " + leftKneeAngle + " Right knee: " + rightKneeAngle);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Parse error: " + e.Message);
        }
    }

    Vector3 ExtractLandmark(string json, string index)
    {
        string searchKey = "\"" + index + "\"";
        int startIdx = json.IndexOf(searchKey);
        if (startIdx == -1) return Vector3.zero;

        int blockStart = json.IndexOf("{", startIdx);
        int blockEnd = json.IndexOf("}", blockStart);
        string block = json.Substring(blockStart, blockEnd - blockStart + 1);

        float x = ExtractFloat(block, "x");
        float y = ExtractFloat(block, "y");
        float z = ExtractFloat(block, "z");

        return new Vector3(x, y, z);
    }

    float ExtractFloat(string block, string key)
    {
        string searchKey = "\"" + key + "\"";
        int idx = block.IndexOf(searchKey);
        int colonIdx = block.IndexOf(":", idx);
        int endIdx = block.IndexOfAny(new char[] { ',', '}' }, colonIdx);
        string value = block.Substring(colonIdx + 1, endIdx - colonIdx - 1).Trim();
        return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    float CalculateAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ba = a - b;
        Vector3 bc = c - b;
        float angle = Vector3.Angle(ba, bc);
        return angle;
    }

    void OnDestroy()
    {
        isRunning = false;
        if (udpClient != null) udpClient.Close();
    }
}