using UnityEngine;

// ザコ敵の基本クラス（移動・攻撃・被弾・アイテムドロップ）。
// 画面上に一番大量に出現するオブジェクトなので、こいつが弾やエフェクトを毎回
// Instantiateすると激重になる。そのため、弾などは全てプール（BulletManager）から借りている。
public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("Attack Settings")]
    [SerializeField] private float fireRate = 2.0f;
    private float fireTimer = 0f;

    [Header("Score & Drop Settings")]
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private GameObject healItemPrefab;
    [SerializeField] private GameObject powerUpItemPrefab;
    [SerializeField] [Range(0f, 100f)] private float healDropChance = 10f;  
    [SerializeField] [Range(0f, 100f)] private float powerUpDropChance = 5f; 

    void Update()
    {
        // ひたすら下に向かって進む
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }

        // 画面の下端（見えない位置）まで行ったら自分自身を消す。
        // これを忘れると、画面外の見えない敵が無限にメモリに溜まってしまいゲームが落ちる。
        if (transform.position.y < -15.0f)
        {
            Destroy(gameObject);
        }
    }

    private void Shoot()
    {
        // 弾を撃つ処理。
        // 重いInstantiateは使わず、BulletManagerに頼んでプールしてある弾を借りてくる。
        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.SpawnEnemyBullet(transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーの弾が当たった時の処理
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            // 自機弾はプールで使い回す仕組みなので、Destroyで完全に消さずに非アクティブ化してプールへ返す
            collision.gameObject.SetActive(false);

            // 爆発エフェクトも毎回作ると重いのでプールから引っ張ってくる
            if (BulletManager.Instance != null)
            {
                BulletManager.Instance.SpawnExplosion(transform.position, Quaternion.identity);
            }

            if (ScoreManager.instance != null) ScoreManager.instance.AddScore(scoreValue);

            DropItem();

            if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayExplosionSound();

            Destroy(gameObject); 
        }
    }

    // 倒された時のアイテムドロップ判定
    private void DropItem()
    {
        // まずパワーアップの抽選をする。
        // 当たった場合はreturnで処理を抜ける（パワーアップと回復が同時にドロップして重なるのを防ぐため）
        if (powerUpItemPrefab != null && Random.Range(0f, 100f) <= powerUpDropChance)
        {
            Instantiate(powerUpItemPrefab, transform.position, Quaternion.identity);
            return; 
        }

        // パワーアップが落ちなかった場合のみ、回復アイテムの抽選を行う
        if (healItemPrefab != null && Random.Range(0f, 100f) <= healDropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }
    }
}