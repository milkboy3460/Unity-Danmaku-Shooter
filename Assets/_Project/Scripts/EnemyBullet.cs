using UnityEngine;

/// <summary>
/// 敵が発射する汎用的な弾のクラス。
/// 生成元（ボスやザコ敵）から進行方向や見た目（属性色）を動的に設定される前提で動作する。
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 5.0f;
    
    // 攻撃属性に応じたスプライトのバリエーション（0:赤, 1:緑, 2:青）
    [SerializeField] private Sprite[] bulletSprites; 
    
    private Vector3 moveDirection = Vector3.down;
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 弾の見た目（色）を設定する。
    /// Instantiate直後など、Start()が走る前に外部から呼ばれるケースを想定し、
    /// 必要に応じてSpriteRendererを動的に取得（遅延初期化）してエラーを防ぐ。
    /// </summary>
    /// <param name="type">スプライト配列のインデックス</param>
    public void SetBulletSprite(int type)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (bulletSprites != null && type >= 0 && type < bulletSprites.Length)
        {
            spriteRenderer.sprite = bulletSprites[type];
        }
    }

    /// <summary>
    /// 弾の進行方向を設定する。
    /// 確実に指定速度で等速直線運動を行わせるため、受け取ったベクトルを正規化（Normalize）して保持する。
    /// </summary>
    /// <param name="dir">進行方向のベクトル</param>
    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
    }

    void Update()
    {
        // フレームレートに依存しない等速直線運動
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnBecameInvisible()
    {
        // 画面外に出た不要な弾を自動破棄し、メモリリーク（処理落ち）を防止する
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // ダメージ計算などの状態変化はPlayer側の責務とするため、
            // 弾のスクリプトでは「当たったら自身を消滅させる」処理のみを行う（関心の分離）
            Destroy(gameObject);
        }
    }
}