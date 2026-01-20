using UnityEngine;
using TMPro; // TextMeshProを使うために必要

public class ScoreManager : MonoBehaviour
{
    // どこからでも ScoreManager.instance でアクセスできるようにする
    public static ScoreManager instance;

    public TextMeshProUGUI _scoreText; // 画面上のテキスト
    private int _currentScore = 0;

    private void Awake()
    {
        // インスタンスの登録
        if (instance == null) instance = this;
    }

    private void Start()
    {
        UpdateScoreDisplay();
    }

    // スコアを加算する関数
    public void AddScore(int amount)
    {
        _currentScore += amount;
        UpdateScoreDisplay();
    }

    // 表示を更新する関数
    void UpdateScoreDisplay()
    {
        if (_scoreText != null)
        {
            _scoreText.text = "SCORE: " + _currentScore.ToString();
        }
    }
}