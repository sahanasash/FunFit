using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VestVisualization : MonoBehaviour
{
    private Image[] motorDots;
    private int rows = 6;
    private int cols = 4;
    private Color offColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    private Color onColor = new Color(0f, 1f, 0.5f, 1f);

    private RectTransform vestPanel;

    void Start()
    {
        CreateVestUI();
    }

    void CreateVestUI()
    {
        GameObject panelObj = new GameObject("VestPanel");
        panelObj.transform.SetParent(transform, false);
        vestPanel = panelObj.AddComponent<RectTransform>();
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        vestPanel.anchorMin = new Vector2(1, 0.5f);
        vestPanel.anchorMax = new Vector2(1, 0.5f);
        vestPanel.pivot = new Vector2(1, 0.5f);
        vestPanel.anchoredPosition = new Vector2(-20, 0);
        vestPanel.sizeDelta = new Vector2(200, 350);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "bHaptics Vest";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 16;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -5);
        titleRect.sizeDelta = new Vector2(0, 30);

        motorDots = new Image[rows * cols];
        float dotSize = 30f;
        float spacingX = 40f;
        float spacingY = 45f;
        float startX = -(cols - 1) * spacingX / 2f;
        float startY = (rows - 1) * spacingY / 2f - 20f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                GameObject dotObj = new GameObject("Motor_" + index);
                dotObj.transform.SetParent(panelObj.transform, false);

                Image dotImage = dotObj.AddComponent<Image>();
                dotImage.color = offColor;

                RectTransform dotRect = dotObj.GetComponent<RectTransform>();
                dotRect.anchoredPosition = new Vector2(
                    startX + col * spacingX,
                    startY - row * spacingY
                );
                dotRect.sizeDelta = new Vector2(dotSize, dotSize);

                dotObj.AddComponent<Outline>().effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);

                motorDots[index] = dotImage;
            }
        }
    }

    public void UpdateFromAnimation(float normalizedTime)
    {
        float cycleTime = normalizedTime % 1f;

        if (cycleTime < 0.5f)
        {
            // Going down - map 0 to 0.5 onto rows top to bottom
            float progress = cycleTime / 0.5f;
            int activeRow = Mathf.FloorToInt(progress * rows);
            activeRow = Mathf.Clamp(activeRow, 0, rows - 1);
            SetActiveRow(activeRow);
        }
        else
        {
            // Coming up - map 0.5 to 1.0 onto rows bottom to top
            float progress = (cycleTime - 0.5f) / 0.5f;
            int activeRow = rows - 1 - Mathf.FloorToInt(progress * rows);
            activeRow = Mathf.Clamp(activeRow, 0, rows - 1);
            SetActiveRow(activeRow);
        }
    }

    private void SetActiveRow(int activeRow)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                motorDots[index].color = (row == activeRow) ? onColor : offColor;
            }
        }
    }
    public void ShowOutOfSync()
{
    Color alertColor = new Color(1f, 0f, 0f, 1f);
    for (int i = 0; i < motorDots.Length; i++)
    {
        motorDots[i].color = alertColor;
    }
}
}