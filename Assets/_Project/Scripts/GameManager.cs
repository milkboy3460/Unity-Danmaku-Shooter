using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; 

/// <summary>
/// ゲームの進行状態と、ゲームオーバー時のUI遷移・スコア送信を統括するマネージャークラス。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI; 
    [SerializeField] private RectTransform arrow;    
    [SerializeField] private RectTransform yesPos;   
    [SerializeField] private RectTransform noPos;    
    [SerializeField] private GameObject hpBarObject; 

    [Header("Effects")]
    [SerializeField] private BlinkEffect yesEffect;   
    [SerializeField] private BlinkEffect noEffect;    
    [SerializeField] private BlinkEffect arrowEffect; 

    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip moveSound;   
    [SerializeField] private AudioClip submitSound; 
    private AudioSource uiAudioSource;

    private bool isGameOver = false;
    private int selectedIndex = 0; 
    
    // 連続入力を防ぐためのフラグ
    private bool isProcessingInput = false; 

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
        Time.timeScale = 1f;

        uiAudioSource = GetComponent<AudioSource>();
        
        // Time.timeScale = 0（ポーズ状態）でもUIの効果音を鳴らすための設定
        uiAudioSource.ignoreListenerPause = true;
    }

    void Update()
    {
        if (!isGameOver || isProcessingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = 0; 
            UpdateMenuVisuals();
            PlaySound(moveSound);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = 1; 
            UpdateMenuVisuals();
            PlaySound(moveSound);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySound(submitSound);
            StartCoroutine(SubmitRoutine());
        }
    }

    /// <summary>
    /// メニュー選択時の処理。
    /// SEが鳴り終わるまでの体感時間を考慮し、少し待機してからシーン遷移を行う。
    /// </summary>
    private IEnumerator SubmitRoutine()
    {
        isProcessingInput = true;
        
        // タイムスケールに依存しない待機
        yield return new WaitForSecondsRealtime(0.2f);
        
        Time.timeScale = 1f;
        if (ScoreManager.instance != null) ScoreManager.instance.ResetScore();

        if (selectedIndex == 0) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else SceneManager.LoadScene("StartScene");
    }

    public void StartGameOverSequence()
    {
        StartCoroutine(GameOverRoutine());
    }

    /// <summary>
    /// ゲームオーバー時の演出シーケンス。
    /// 撃墜直後にスローモーション効果（ヒットストップ）を入れ、プレイヤーに状況を認識させる間を作る。
    /// </summary>
    private IEnumerator GameOverRoutine()
    {
        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.StopBGM();
        }

        // 演出目的で一時的にタイムスケールを下げる
        Time.timeScale = 0.3f;
        
        // スローモーション中でも実時間で待機する
        yield return new WaitForSecondsRealtime(1.5f);
        
        ShowGameOver();
    }

    /// <summary>
    /// ゲームを完全に停止させ、リザルト画面の表示とサーバーへのスコア送信を行う。
    /// </summary>
    private void ShowGameOver()
    {
        isGameOver = true;
        
        // ゲーム内時間を完全に停止
        Time.timeScale = 0f; 
        
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (hpBarObject != null) hpBarObject.SetActive(false);

        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.PlayGameOverBGM();
        }

        int finalScore = 0;
        if (ScoreManager.instance != null)
        {
            finalScore = ScoreManager.instance.GetCurrentScore();
            if (gameOverScoreText != null)
            {
                // UI表示用の文字列フォーマット
                gameOverScoreText.text = $"YOUR SCORE: {finalScore:D6}";
            }
        }

        // サーバー連携：バックエンドAPIへ最終スコアを送信
        RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
        if (rankingManager != null)
        {
            rankingManager.PostFinalScore(finalScore);
        }
        else
        {
            Debug.LogWarning("RankingNetworkManagerが見つかりません。");
        }
        
        selectedIndex = 0;
        UpdateMenuVisuals(); 
    }

    private void UpdateMenuVisuals()
    {
        if (arrow == null) return;

        if (selectedIndex == 0) // YES
        {
            arrow.position = yesPos.position;
            if (yesEffect != null) yesEffect.SetSelected(true);
            if (noEffect != null) noEffect.SetSelected(false);
        }
        else // NO
        {
            arrow.position = noPos.position;
            if (yesEffect != null) yesEffect.SetSelected(false);
            if (noEffect != null) noEffect.SetSelected(true);
        }
        if (arrowEffect != null) arrowEffect.SetSelected(true);
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }
}