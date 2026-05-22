using UnityEngine;
using TMPro;

// スコア計算とハイスコアの保存（永続化）を管理するクラス。
// ゲーム中どこからでもスコアを足せるようにシングルトンにしている。
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
        instance = this;
        
        // 起動時、端末内に保存されているハイスコアを引っ張り出す
        hiScore = PlayerPrefs.GetInt(hiScoreKey, 0);
    }

    void Start()
    {
        UpdateScoreText();
        UpdateHiScoreText();
    }

    // スコア加算処理。
    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        UpdateScoreText();

        // 現在のスコアがハイスコアを抜いた時だけ更新する
        if (currentScore > hiScore)
        {
            hiScore = currentScore;
            UpdateHiScoreText();
            
            // 【重要】
            // アプリが突然落ちてもデータが消えないように、更新のたびにローカルへ即保存(Save)する
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
            // アーケードゲームっぽく、6桁のゼロ埋め（D6）にして表示を整える
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

    // リトライ時などにスコアを0に戻す
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
    }
}