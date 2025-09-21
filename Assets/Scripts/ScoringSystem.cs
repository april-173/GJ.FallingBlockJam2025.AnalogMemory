using TMPro;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public string scorePrefix = "Score: ";
    public TextMeshProUGUI linesText;
    public string linesPrefix = "Lines: ";

    [Header("Score Settings")]
    public int scorePerLine = 100;
    public int softDropScorePerCell = 2;
    public int hardDropScorePerCell = 5;
    public float scoreIncreaseSpeed = 200f;

    private int currentScore = 0;
    private int targetScore = 0;
    private int currentLines = 0;
    private int targetLines = 0;

    void Start()
    {
        UpdateScoreUI();
        UpdateLinesUI();
    }

    void Update()
    {
        // 平滑增加分数
        if (currentScore < targetScore)
        {
            currentScore += Mathf.CeilToInt(scoreIncreaseSpeed * Time.deltaTime);
            if (currentScore > targetScore)
                currentScore = targetScore;
            UpdateScoreUI();
        }

        // 平滑增加行数
        if (currentLines < targetLines)
        {
            currentLines += 1;
            if (currentLines > targetLines)
                currentLines = targetLines;
            UpdateLinesUI();
        }
    }

    public void Reset()
    {
        currentScore = 0;
        targetScore = 0;
        currentLines = 0;
        targetLines = 0;
        UpdateScoreUI();
        UpdateLinesUI();
    }

    public void HandleSoftDrop(int count = 1)
    {
        AddScore(softDropScorePerCell * count);
    }

    public void HandleHardDrop(int count)
    {
        AddScore(hardDropScorePerCell * count);
    }

    private void AddScore(int amount)
    {
        targetScore += amount;
    }

    public void AddLines(int count)
    {
        targetLines += count;
        AddScore(count * scorePerLine);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = scorePrefix + currentScore.ToString("D8");
    }

    private void UpdateLinesUI()
    {
        if (linesText != null)
            linesText.text = linesPrefix + currentLines.ToString("D8");
    }
}
