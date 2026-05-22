using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; 
using TMPro;
using System.Collections;

// タイトル画面のUI操作と、プレイヤー名の登録・シーン遷移を管理するクラス。
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

    [Header("Visual Effects")]
    [SerializeField] private BlinkEffect startEffect;    
    [SerializeField] private BlinkEffect rankingEffect;  
    [SerializeField] private BlinkEffect statsEffect;    

    [Header("Audio Settings")]
    [SerializeField] private AudioClip moveSound;   
    [SerializeField] private AudioClip submitSound; 
    [SerializeField] private AudioClip errorSound;  
    
    private AudioSource audioSource;

    public static bool IsNameConfirmed = false;
    private int currentIndex = 0; 
    private int maxMenuIndex = 2; // 0:START, 1:RANKING, 2:STATS
    
    private bool canInteractMenu = false; 
    private bool isPopupOpen = false; 

    void Start()
    {
        Time.timeScale = 1f;
        audioSource = GetComponent<AudioSource>();
        
        if (rankingPopup != null) rankingPopup.SetActive(false);
        if (statsPopup != null) statsPopup.SetActive(false);
        if (errorText != null) errorText.text = "";

        // 前回の起動時に使った名前をローカルからロードしておく（ユーザーの入力の手間を減らすUX）
        string savedName = PlayerPrefs.GetString("SavedPlayerName", "");
        PlayerNameManager.PlayerName = savedName;

        if (!string.IsNullOrEmpty(savedName))
        {
            // すでに名前が登録済みなら、入力画面を飛ばしてすぐにメニューを操作できるようにする
            IsNameConfirmed = true;
            canInteractMenu = true; 
            if (nameInputField != null) nameInputField.gameObject.SetActive(false);
            SetMenuColor(Color.white);
            
            currentIndex = 0;
            UpdateSelection(); 
        }
        else
        {
            // 初回起動時は名前入力を強制させる
            IsNameConfirmed = false; 
            canInteractMenu = false;
            SetMenuColor(new Color(0.3f, 0.3f, 0.3f, 1f)); // 名前未入力時はメニューを暗くして操作不可にする
            
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
        if (nameInputField != null)
        {
            nameInputField.onSubmit.RemoveListener(OnNameSubmit);
        }
    }

    void Update()
    {
        // 【デバッグ用】Rキーで名前セーブを消して最初からやり直せるようにしておく
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteKey("SavedPlayerName");
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (!IsNameConfirmed) return;

        // ポップアップが出ている時はメニュー操作をブロックする
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

        if (!canInteractMenu)
        {
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return))
            {
                canInteractMenu = true;
            }
            return; 
        }

        // キー入力でメニュー移動（上下）
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

        // 決定操作
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySound(submitSound);
            ExecuteSelection();
        }
    }

    // 選択肢の点滅（BlinkEffect）を切り替える
    private void UpdateSelection()
    {
        if (!IsNameConfirmed) return;

        if (startEffect != null) startEffect.SetSelected(currentIndex == 0);
        if (rankingEffect != null) rankingEffect.SetSelected(currentIndex == 1);
        if (statsEffect != null) statsEffect.SetSelected(currentIndex == 2);
    }

    // 名前を入力してEnterを押した時の処理
    private void OnNameSubmit(string text)
    {
        if (text.Length > 0)
        {
            string upperName = text.ToUpper();
            nameInputField.interactable = false; // 送信中は連打できないようにする

            // サーバーに名前を登録して重複チェックを行う
            RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
            if (rankingManager != null)
            {
                rankingManager.RegisterPlayer(upperName, (isSuccess) =>
                {
                    if (isSuccess)
                    {
                        PlayerNameManager.PlayerName = upperName;
                        PlayerPrefs.SetString("SavedPlayerName", upperName);
                        PlayerPrefs.Save();
                        StartCoroutine(SafeTransitionToMenu()); 
                    }
                    else
                    {
                        // サーバー側で名前が既に使われていたらエラーを表示
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
            // 名前が空ならフォーカスを戻して再入力させる
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    // 遷移時に発生するEventSystemのバグを回避するため、少しだけ待ってから遷移する
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

    private void ExecuteSelection()
    {
        if (currentIndex == 0) SceneManager.LoadScene("MainScene"); 
        else if (currentIndex == 1)
        {
            isPopupOpen = true;
            if (rankingPopup != null) rankingPopup.SetActive(true);
            RankingNetworkManager rankingManager = FindObjectOfType<RankingNetworkManager>();
            if (rankingManager != null) rankingManager.FetchRanking();
        }
        else if (currentIndex == 2)
        {
            isPopupOpen = true;
            if (statsPopup != null) statsPopup.SetActive(true);
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