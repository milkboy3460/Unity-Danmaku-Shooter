using UnityEngine;
using UnityEngine.Pool; // GC（ゴミ集め）でカクつくのを防ぐためのプーリング用

// 敵や中ボスが撃ってくる弾のクラス。
// 大量に出るので、いちいちDestroyせずにプール（BulletManager）に返して使い回す設計。
public class EnemyBullet : MonoBehaviour
{
    // 自分が帰るプール
    private IObjectPool<EnemyBullet> managedPool;

    [Header("Settings")]
    [SerializeField] private float speed = 5.0f;
    
    // 中ボスが撃つ弾の色分け用（0:赤, 1:緑, 2:青）
    [SerializeField] private Sprite[] bulletSprites; 
    
    private Vector3 moveDirection = Vector3.down;
    private SpriteRenderer spriteRenderer;

    // どこに帰るか（プール元）をセットする
    public void SetPool(IObjectPool<EnemyBullet> pool)
    {
        managedPool = pool;
    }

    // 弾の見た目（色）を変える処理。
    // プールから出た直後（Startが走る前）に呼ばれることがあるので、
    // ここで毎回nullチェックしてエラー（NullReferenceException）を防いでいる。
    public void SetBulletSprite(int type)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (bulletSprites != null && type >= 0 && type < bulletSprites.Length)
        {
            spriteRenderer.sprite = bulletSprites[type];
        }
    }

    // 飛んでいく方向をセットする。
    // 斜めに撃ち出された時に弾の速度が速くならないように、必ず正規化（Normalize）しておく。
    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
    }

    void Update()
    {
        // フレームレートに依存しないようにTime.deltaTimeを掛けて進ませる
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnBecameInvisible()
    {
        // 画面の外に出たら、もう見えないのでプールに回収する
        ReleaseToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーに当たった時のダメージ計算やエフェクト生成は、ここではなくPlayer側に任せる。
            // 弾の役割は「当たったら消える（プールに帰る）」ことだけに専念させて、コードを疎結合に保つ。
            ReleaseToPool();
        }
    }

    // プールへ戻す共通処理
    private void ReleaseToPool()
    {
        if (managedPool != null)
        {
            managedPool.Release(this);
        }
        else
        {
            // プールを使わずに直接Instantiateしてテストした時などのための保険
            Destroy(gameObject);
        }
    }
}