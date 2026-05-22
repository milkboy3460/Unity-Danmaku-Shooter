using UnityEngine;
using UnityEngine.UI;

// 画面上のHPバー（ハートアイコン）の見た目を更新するだけのクラス。
// HPの数値管理はPlayer側に任せ、UI側は「言われた数だけ赤くする」という
// 受動的な役割（Passive View）に徹することで、ロジックと描画をきっちり分離している。
public class HPDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] heartImages;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite redHeart;
    [SerializeField] private Sprite blackHeart;

    // Player側でダメージを受けたり回復したりした時に、セットで呼ばれる処理。
    public void UpdateHP(int currentHP)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            // 今のHPの数までは赤いハート、それ以降は黒いハート（空っぽ）の画像を割り当てる
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