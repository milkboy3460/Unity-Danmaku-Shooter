using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの現在HPに応じて、UI上のライフ（ハートアイコン）の表示状態を管理するクラス。
/// </summary>
public class HPDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] heartImages;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite redHeart;
    [SerializeField] private Sprite blackHeart;

    /// <summary>
    /// 現在のHP値を受け取り、UIの表示を同期する。
    /// Player側のダメージ処理や回復処理のコールバックとして呼ばれる想定。
    /// </summary>
    /// <param name="currentHP">更新後の現在HP値</param>
    public void UpdateHP(int currentHP)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            // インデックスが現在HPの範囲内であれば点灯、範囲外であれば消灯状態のスプライトを割り当てる
            if (i < currentHP)
            {
                heartImages[i].sprite = redHeart;
            }
            else
            {
                heartImages[i].sprite = blackHeart;
            }
        }
    }
}