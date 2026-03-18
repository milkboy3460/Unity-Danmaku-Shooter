using UnityEngine;

/// <summary>
/// ランキングポップアップの表示・非表示（アクティブ状態）を制御するUI管理クラス。
/// </summary>
public class RankingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject rankingPopup;

    void Start()
    {
        // 初期状態の不整合を防ぐため、開始時に明示的に非表示に設定
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(false);
        }
    }

    /// <summary>
    /// ランキング画面を開く。
    /// タイトル画面やリザルト画面のボタンイベントから呼び出されるエントリーポイント。
    /// </summary>
    public void OpenRanking()
    {
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(true);
        }
    }

    /// <summary>
    /// ランキング画面を閉じる。
    /// ポップアップ内の「閉じる」ボタンやキャンセル操作に割り当てられる。
    /// </summary>
    public void CloseRanking()
    {
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(false);
        }
    }
}