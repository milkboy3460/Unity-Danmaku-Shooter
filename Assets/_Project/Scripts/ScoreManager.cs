using UnityEngine;
using TMPro;

/// <summary>
/// スコアの計算およびハイスコアの永続化を管理するマネージャークラス。
/// シングルトンパターンにより、ゲーム内のどこからでもスコア加算を可能にする。
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText; 
    [SerializeField] private TextMeshProUGUI hiScoreText; 

    [Header("Settings")]
    [SerializeField] private string hiScoreKey = "HiScore";

    private int currentScore = 0; 
    private static int hiScore = 0; 

    void Awake()
    {
        // 簡易的なシングルトンの初期化
        instance = this;
        
        // 起動時に保存済みのハイスコアをロードする
        hiScore = PlayerPrefs.GetInt(hiScoreKey, 0);
    }

    void Start()
    {
        UpdateScoreText();
        UpdateHiScoreText();
    }

    /// <summary>
    /// スコアを加算し、必要に応じてハイスコアを更新・保存する。
    /// </summary>
    /// <param name="scoreToAdd">加算するスコア値</param>
    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        UpdateScoreText();

        // 現在のスコアがハイスコアを更新した際、リアルタイムで保存処理を行う
        if (currentScore > hiScore)
        {
            hiScore = currentScore;
            UpdateHiScoreText();
            
            // PlayerPrefsを用いてローカルにデータを永続化
            PlayerPrefs.SetInt(hiScoreKey, hiScore);
            PlayerPrefs.Save();
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            // 6桁のゼロ埋め（D6）でアーケードライクな表示形式に整える
            scoreText.text = "SCORE: " + currentScore.ToString("D6");
        }
    }

    private void UpdateHiScoreText()
    {
        if (hiScoreText != null)
        {
            hiScoreText.text = "HI SCORE: " + hiScore.ToString("D6");
        }
    }

    /// <summary>
    /// 現在のスコアをリセットする。リトライ時などの呼び出しを想定。
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
    }
}