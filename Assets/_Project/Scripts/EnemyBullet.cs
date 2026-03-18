using UnityEngine;
using UnityEngine.Pool; // GCアロケーションを防ぐためのオブジェクトプーリング用

/// <summary>
/// 敵キャラクターが発射する弾の制御クラス。
/// メモリの動的確保/解放によるGCスパイクを防ぐため、UnityEngine.Poolによる再利用を前提とする。
/// 進行方向や属性色などは生成元から動的に注入される設計。
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    // 自身を管理しているオブジェクトプールへの参照
    private IObjectPool<EnemyBullet> managedPool;

    [Header("Settings")]
    [SerializeField] private float speed = 5.0f;
    
    // 攻撃属性に応じたスプライトのバリエーション（0:赤, 1:緑, 2:青）
    [SerializeField] private Sprite[] bulletSprites; 
    
    private Vector3 moveDirection = Vector3.down;
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// プール管理元から自身の参照先を注入する。
    /// </summary>
    public void SetPool(IObjectPool<EnemyBullet> pool)
    {
        managedPool = pool;
    }

    /// <summary>
    /// 弾の属性（見た目）を初期化する。
    /// PoolからGetされた直後（Start実行前）に外部から呼ばれるケースを考慮し、
    /// SpriteRendererの遅延評価による取得を行ってNullReferenceを防ぐ。
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
    /// 進行方向ベクトルを設定する。
    /// 斜め方向でも速度が変化しないよう、正規化（Normalize）して保持する。
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
        // 画面外へ出たオブジェクトはDestroyせず、プールへ返却して再利用待機状態にする
        ReleaseToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーへのダメージ計算やエフェクト生成の責務はPlayer/Manager側で持つ（関心の分離）。
            // 弾自身は「ヒットしたら自身を非アクティブ化してプールへ戻る」処理のみに専念する。
            ReleaseToPool();
        }
    }

    /// <summary>
    /// 自身をプールへ返却する共通処理。
    /// </summary>
    private void ReleaseToPool()
    {
        if (managedPool != null)
        {
            managedPool.Release(this);
        }
        else
        {
            // 単体テスト等でプールを経由せずにInstantiateされた場合のフェイルセーフ
            Destroy(gameObject);
        }
    }
}