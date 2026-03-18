using UnityEngine;

/// <summary>
/// プレイヤー名をシーン間で共有・保持するためのデータ管理クラス。
/// シングルトンパターンを用いて、ゲームの起動から終了まで一貫したデータアクセスを提供する。
/// </summary>
public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance { get; private set; }
    
    /// <summary>
    /// 各シーンや通信クラス（RankingNetworkManager等）から参照されるプレイヤー名。
    /// </summary>
    public static string PlayerName { get; set; } = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // シーン遷移時にオブジェクトが破棄されるのを防ぎ、入力された名前をメモリ上に永続化する
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 重複したマネージャーが存在しないよう、自身を破棄（シングルトンの保証）
            Destroy(gameObject);
        }
    }
}