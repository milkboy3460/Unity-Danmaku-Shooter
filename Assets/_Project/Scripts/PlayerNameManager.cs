using UnityEngine;

// タイトル画面で入力した「プレイヤー名」を、ゲームオーバー時のランキング送信まで消さずに持ち越すためのクラス。
// Unityはシーンを移動するとGameObjectが全部破棄されてデータが飛んでしまうため、
// シングルトン化 ＋ DontDestroyOnLoad の組み合わせでデータを保護している。
public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance { get; private set; }
    
    // ランキング送信時（RankingNetworkManager）にどこからでもサクッと取れるように static にしておく
    public static string PlayerName { get; set; } = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 【重要】
            // これを書かないと、ゲーム本編のシーンに飛んだ瞬間に名前のデータが消滅してしまい、
            // サーバー（DB）にランキングをPOSTする際に全部「空っぽ（名無し）」で送られてしまう。
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ゲームオーバーからタイトル画面に戻ってきた時などに、Managerが2つに増殖してしまうバグを防ぐ
            Destroy(gameObject);
        }
    }
}