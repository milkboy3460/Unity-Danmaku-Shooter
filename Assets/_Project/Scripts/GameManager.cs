using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; 
using TMPro;
using System.Collections; 

// ゲーム進行と、ゲームオーバー時のUI・スコア送信周りを一手に引き受けるマネージャー。
// （※以前は矢印アイコンを動かしていたが、UIとして野暮ったかったので点滅エフェクトのみで選択させる仕様に変更した）
[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI; 
    [SerializeField] private GameObject hpBarObject; 

    [Header("Visual Effects (点滅のみ使用)")]
    [SerializeField] private BlinkEffect yesEffect;   
    [SerializeField] private BlinkEffect noEffect;    

    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip moveSound;   
    [SerializeField] private AudioClip submitSound; 
    private AudioSource uiAudioSource;

    private bool isGameOver = false;
    private int selectedIndex = 0; 
    
    // エンターキー連打による多重決定（シーンの二重ロードやスコアの二重送信バグ）を防ぐためのフラグ
    private bool isProcessingInput = false; 

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
        Time.timeScale = 1f;

        uiAudioSource = GetComponent<AudioSource>();
        
        // 【重要】ゲームオーバー時は Time.timeScale = 0（停止状態）になるため、
        // これをtrueにしておかないとUIのカーソル音や決定音まで一緒に止まってしまう。
        uiAudioSource.ignoreListenerPause = true;
    }

    void Update()
    {
        if (!isGameOver || isProcessingInput) return;

        // 左右キー（またはADキー）で選択切り替え
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = 0; // YES
            UpdateMenuVisuals();
            PlaySound(moveSound);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = 1; // NO
            UpdateMenuVisuals();
            PlaySound(moveSound);
        }

        // 決定（スペースまたはエンター）
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySound(submitSound);
            StartCoroutine(SubmitRoutine());
        }
    }

    private IEnumerator SubmitRoutine()
    {
        // 入力受付をブロックして、連打バグを防止する
        isProcessingInput = true;
        
        // Time.timeScaleが0（ポーズ状態）なので、通常の WaitForSeconds だと一生待機が終わらない。
        // SE（決定音）を少し聞かせてから遷移させたいので、現実時間ベースの WaitForSecondsRealtime を使う。
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

    private IEnumerator GameOverRoutine()
    {
        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.StopBGM();
        }

        // 【ヒットストップ演出】
        // プレイヤーが死んだ瞬間に少しだけスローモーションにすることで「やられた感」を強調する
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(1.5f);
        
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f; // 完全にゲーム進行を止める
        
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
                gameOverScoreText.text = $"YOUR SCORE: {finalScore:D6}";
            }
        }

        // 自作のバックエンド（Javaサーバー）へ最終スコアをPOSTしてランキングに反映させる
        RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
        if (rankingManager != null)
        {
            rankingManager.PostFinalScore(finalScore);
        }
        
        selectedIndex = 0;
        UpdateMenuVisuals(); 
    }

    // 矢印の座標移動をやめて、UIの点滅エフェクトのオンオフだけで選択状態を表現する
    private void UpdateMenuVisuals()
    {
        // 選択中の要素を光らせ、そうでない方の点滅を止める
        if (yesEffect != null) yesEffect.SetSelected(selectedIndex == 0);
        if (noEffect != null) noEffect.SetSelected(selectedIndex == 1);
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }
}