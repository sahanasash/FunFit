using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private RawImage rawImage;
    private UdpClient udpClient;
    private Texture2D smallTexture;
    private RenderTexture rt;
    private float sendTimer = 0f;
    private float sendInterval = 0.033f;
    private bool isReady = false;

    void Start()
    {
        GameObject panelObj = new GameObject("WebcamPanel");
        panelObj.transform.SetParent(transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(320, 240);

        rawImage = panelObj.AddComponent<RawImage>();

        webcamTexture = new WebCamTexture();
        webcamTexture.requestedWidth = 640;
        webcamTexture.requestedHeight = 480;
        webcamTexture.Play();

        rawImage.texture = webcamTexture;

        udpClient = new UdpClient();
        smallTexture = new Texture2D(320, 240, TextureFormat.RGB24, false);
        rt = new RenderTexture(320, 240, 0);
    }

    void Update()
{
    if (webcamTexture == null || !webcamTexture.isPlaying)
    {
        return;
    }

    if (!isReady)
    {
        isReady = true;
        Debug.Log("Webcam ready: " + webcamTexture.width + "x" + webcamTexture.height);
    }

    sendTimer += Time.deltaTime;
    if (sendTimer >= sendInterval)
    {
        sendTimer = 0f;
        SendFrameToPython();
    }
}

    void SendFrameToPython()
    {
        try
        {
            Texture2D fullTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
            fullTexture.SetPixels(webcamTexture.GetPixels());
            fullTexture.Apply();

            Graphics.Blit(fullTexture, rt);
            RenderTexture.active = rt;
            smallTexture.ReadPixels(new Rect(0, 0, 320, 240), 0, 0);
            smallTexture.Apply();
            RenderTexture.active = null;

            Destroy(fullTexture);

            byte[] jpgData = smallTexture.EncodeToJPG(10);
            Debug.Log("Sending frame: " + jpgData.Length + " bytes");

            if (jpgData.Length < 65000)
            {
                udpClient.Send(jpgData, jpgData.Length, "127.0.0.1", 5066);
            }
            else
            {
                Debug.LogWarning("Frame too large: " + jpgData.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Frame send error: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();
        if (udpClient != null)
            udpClient.Close();
        if (rt != null)
            rt.Release();
    }
}