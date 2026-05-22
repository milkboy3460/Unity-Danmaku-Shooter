using UnityEngine;

// 回復やパワーアップなど、ドロップアイテム全般の動きを制御するクラス。
// アイテムの種類ごとにわざわざ別のスクリプトを作ると管理が面倒になるので、
// Enum（ItemType）を使って、インスペクタから手軽に種類を切り替えられるようにまとめている。
public class Item : MonoBehaviour
{
    public enum ItemType
    {
        Heal,     // 体力回復
        PowerUp   // 攻撃力強化
    }

    [Header("Item Settings")]
    [SerializeField] private ItemType type = ItemType.Heal; 
    [SerializeField] private float moveSpeed = 2.0f;

    void Update()
    {
        // ひたすら下へ落ちていく
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        // 取り逃がしたアイテムが画面外に溜まり続けてメモリを食いつぶすのを防ぐため、
        // 見えなくなったら確実にDestroyして掃除する
        if (transform.position.y < -10.0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            
            // 【防衛的プログラミング】
            // 万が一「Player」タグが付いているのにPlayerスクリプトが付いていない
            // 想定外のオブジェクトに当たった場合でも、NullReferenceエラーで落ちないようにしておく。
            if (player != null)
            {
                ApplyEffect(player);
            }

            // 効果を与えたら自分自身は消滅する
            Destroy(gameObject);
        }
    }

    // アイテムの種類に応じて、Player側のメソッドを叩く。
    // （※実際の回復処理やパワーアップの仕様はPlayerクラス側に書くことで、
    // 　アイテム側がPlayerの内部実装を深く知らなくて済むように疎結合にしている）
    private void ApplyEffect(Player player)
    {
        switch (type)
        {
            case ItemType.Heal:
                player.Heal(1);
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.PlayHealSound();
                }
                break;

            case ItemType.PowerUp:
                player.PowerUp();
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.PlayPowerUpSound();
                }
                break;
        }
    }
}