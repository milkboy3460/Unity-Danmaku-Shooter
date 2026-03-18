using UnityEngine;

/// <summary>
/// プレイヤーが取得可能なアイテム（回復・パワーアップ等）の挙動を制御するクラス。
/// </summary>
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
        // フレームレートに依存しない一定速度での落下移動
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        // 画面外（下部）到達時にリソース解放のため自身を破棄
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
            
            // プレイヤーコンポーネントの存在を確認してから種類に応じた効果を適用
            if (player != null)
            {
                ApplyEffect(player);
            }

            // 取得後は即座に自身を破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// アイテム種別に基づいてプレイヤーへ効果を適用し、対応するSEを再生する。
    /// </summary>
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