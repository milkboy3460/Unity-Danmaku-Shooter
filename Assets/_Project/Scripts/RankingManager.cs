using UnityEngine;

// ランキング画面（ポップアップ）の表示・非表示を管理するクラス。
public class RankingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject rankingPopup;

    void Start()
    {
        // 【初期化処理】
        // 万が一、インスペクタ（編集画面）でポップアップを「ON」にしたまま保存してしまっても、
        // ゲーム起動時に強制的に閉じておき、タイトル画面にランキングが被るバグを防ぐ。
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(false);
        }
    }

    // ランキング画面を開く。
    // タイトル画面やゲームオーバー画面の「ランキングボタン」から呼び出される。
    public void OpenRanking()
    {
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(true);
        }
    }

    // ランキング画面を閉じる。
    // ポップアップ内にある「戻る」ボタンなどに割り当てる。
    public void CloseRanking()
    {
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(false);
        }
    }
}