using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; 
using TMPro;
using System.Collections;

/// <summary>
/// タイトル画面のユーザーインターフェースと進行状態を管理するクラス。
/// プレイヤー名の登録、メニュー遷移、ランキング/戦績ポップアップの制御を担う。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TitleUIManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI errorText; 
    public Image startImage;    
    public Image rankingImage;  
    public Image statsImage;    
    public GameObject rankingPopup; 
    public GameObject statsPopup;   

    [Header("Cursor Settings")]
    [SerializeField] private RectTransform arrow;          
    [SerializeField] private RectTransform startPos;        
    [SerializeField] private RectTransform rankingPos;      
    [SerializeField] private RectTransform statsPos;        

    [Header("Visual Effects")]
    [SerializeField] private BlinkEffect startEffect;     
    [SerializeField] private BlinkEffect rankingEffect;   
    [SerializeField] private BlinkEffect statsEffect;     
    [SerializeField] private BlinkEffect arrowEffect;     

    [Header("Audio Settings")]
    [SerializeField] private AudioClip moveSound;   
    [SerializeField] private AudioClip submitSound; 
    [SerializeField] private AudioClip errorSound;  
    
    private AudioSource audioSource;

    // 名前入力済みフラグ（静的プロパティとして管理）
    public static bool IsNameConfirmed = false;
    private int currentIndex = 0; 
    private int maxMenuIndex = 2; // 0:START, 1:RANKING, 2:STATS
    
    private bool canInteractMenu = false; 
    private bool isPopupOpen = false; 

    void Start()
    {
        // アプリケーション全体の状態リセット
        Time.timeScale = 1f;
        audioSource = GetComponent<AudioSource>();
        
        // 初期状態のクリーンアップ
        if (rankingPopup != null) rankingPopup.SetActive(false);
        if (statsPopup != null) statsPopup.SetActive(false);
        if (errorText != null) errorText.text = "";
        if (arrow != null) arrow.gameObject.SetActive(false);

        // ローカルストレージ（PlayerPrefs）から保存済みの名前をロード
        string savedName = PlayerPrefs.GetString("SavedPlayerName", "");
        PlayerNameManager.PlayerName = savedName;

        if (!string.IsNullOrEmpty(savedName))
        {
            // 名前が存在する場合は直接メニュー操作へ移行
            IsNameConfirmed = true;
            canInteractMenu = true; 
            if (nameInputField != null) nameInputField.gameObject.SetActive(false);
            SetMenuColor(Color.white);
            
            currentIndex = 0;
            UpdateSelection(); 
        }
        else
        {
            // 名前が未登録の場合は入力フィールドを優先表示
            IsNameConfirmed = false; 
            canInteractMenu = false;
            SetMenuColor(new Color(0.3f, 0.3f, 0.3f, 1f)); // 未入力時はメニューをグレーアウト
            
            if (nameInputField != null)
            {
                nameInputField.gameObject.SetActive(true); 
                nameInputField.Select();
                nameInputField.ActivateInputField();
                nameInputField.onSubmit.AddListener(OnNameSubmit);
            }
        }
    }

    private void OnDestroy()
    {
        // メモリリーク防止のためイベントリスナーを解除
        if (nameInputField != null)
        {
            nameInputField.onSubmit.RemoveListener(OnNameSubmit);
        }
    }

    void Update()
    {
        // 名前確定前は操作を受け付けない
        if (!IsNameConfirmed) return;

        // ポップアップ（モーダル）表示中の排他制御
        if (isPopupOpen)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                isPopupOpen = false;
                if (rankingPopup != null) rankingPopup.SetActive(false);
                if (statsPopup != null) statsPopup.SetActive(false);
            }
            return;
        }

        // 入力直後の誤操作防止バッファ
        if (!canInteractMenu)
        {
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return))
            {
                canInteractMenu = true;
            }
            return; 
        }

        // キーボード/コントローラーによるメニュー選択制御
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = maxMenuIndex;
            UpdateSelection();
            PlaySound(moveSound);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex > maxMenuIndex) currentIndex = 0;
            UpdateSelection();
            PlaySound(moveSound);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySound(submitSound);
            ExecuteSelection();
        }
    }

    /// <summary>
    /// 現在の選択インデックスに基づいてカーソル位置と視覚エフェクトを更新する。
    /// </summary>
    private void UpdateSelection()
    {
        if (!IsNameConfirmed || arrow == null) return;

        if (currentIndex == 0 && startPos != null) arrow.position = startPos.position;
        else if (currentIndex == 1 && rankingPos != null) arrow.position = rankingPos.position;
        else if (currentIndex == 2 && statsPos != null) arrow.position = statsPos.position;

        arrow.gameObject.SetActive(true);

        // 選択中の要素に対してのみ点滅エフェクト（BlinkEffect）を有効化
        if (startEffect != null) startEffect.SetSelected(currentIndex == 0);
        if (rankingEffect != null) rankingEffect.SetSelected(currentIndex == 1);
        if (statsEffect != null) statsEffect.SetSelected(currentIndex == 2);
        if (arrowEffect != null) arrowEffect.SetSelected(true);
    }

    /// <summary>
    /// 入力されたプレイヤー名のバリデーションおよびサーバー登録を行う。
    /// </summary>
    private void OnNameSubmit(string text)
    {
        if (text.Length > 0)
        {
            string upperName = text.ToUpper();
            
            // 通信中の多重送信（二重登録）を防止するためのUIロック
            nameInputField.interactable = false; 

            RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
            if (rankingManager != null)
            {
                rankingManager.RegisterPlayer(upperName, (isSuccess) =>
                {
                    if (isSuccess)
                    {
                        // サーバー登録成功：ローカルに名前を永続化しメニューへ
                        PlayerNameManager.PlayerName = upperName;
                        PlayerPrefs.SetString("SavedPlayerName", upperName);
                        PlayerPrefs.Save();
                        StartCoroutine(SafeTransitionToMenu()); 
                    }
                    else
                    {
                        // 重複エラー：エラーフィードバックを表示し入力を再開させる
                        if (errorText != null) errorText.text = "NAME ALREADY TAKEN";
                        PlaySound(errorSound);
                        
                        nameInputField.interactable = true;
                        nameInputField.text = "";
                        nameInputField.Select();
                        nameInputField.ActivateInputField();
                    }
                });
            }
        }
        else
        {
            // 空文字送信時のフォールバック
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    /// <summary>
    /// 入力確定後、EventSystemの選択状態をクリアし安全にメニュー操作へ移行させるためのコルーチン。
    /// </summary>
    private IEnumerator SafeTransitionToMenu()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSecondsRealtime(0.1f);

        IsNameConfirmed = true;
        canInteractMenu = true; 

        SetMenuColor(Color.white);
        if (errorText != null) errorText.text = ""; 
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        
        currentIndex = 0;
        UpdateSelection();
    }

    /// <summary>
    /// 選択中のメニュー項目に応じたアクションを実行する。
    /// </summary>
    private void ExecuteSelection()
    {
        if (currentIndex == 0)
        {
            SceneManager.LoadScene("MainScene"); 
        }
        else if (currentIndex == 1)
        {
            isPopupOpen = true;
            if (rankingPopup != null) rankingPopup.SetActive(true);
            
            // 非同期でのランキング取得リクエスト
            RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
            if (rankingManager != null) rankingManager.FetchRanking();
        }
        else if (currentIndex == 2)
        {
            isPopupOpen = true;
            if (statsPopup != null) statsPopup.SetActive(true);
            
            // 非同期での個人戦績取得リクエスト
            RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
            if (rankingManager != null) rankingManager.FetchPlayerStats(PlayerNameManager.PlayerName);
        }
    }

    private void SetMenuColor(Color color)
    {
        if (startImage != null) startImage.color = color;
        if (rankingImage != null) rankingImage.color = color;
        if (statsImage != null) statsImage.color = color; 
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}