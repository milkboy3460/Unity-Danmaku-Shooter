using UnityEngine;

/// <summary>
/// 一般的な敵キャラクターの挙動（移動・攻撃・被弾・ドロップ）を管理するクラス。
/// 大量に出現するため、弾とエフェクトのプーリング化による恩恵が最も大きいクラス。
/// </summary>
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
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }

        // 画面下部の死角領域へ到達した際、オブジェクトを破棄してメモリリークを防ぐ
        if (transform.position.y < -15.0f)
        {
            Destroy(gameObject);
        }
    }

    private void Shoot()
    {
        // 変更点: BulletManagerへ生成を委譲
        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.SpawnEnemyBullet(transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            // 変更点: 自機弾はDestroyせず非アクティブ化し、プールへ返す
            collision.gameObject.SetActive(false);

            // 爆発エフェクトをプールから展開
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

    /// <summary>
    /// 撃破時のアイテムドロップ判定。
    /// </summary>
    private void DropItem()
    {
        if (powerUpItemPrefab != null && Random.Range(0f, 100f) <= powerUpDropChance)
        {
            Instantiate(powerUpItemPrefab, transform.position, Quaternion.identity);
            return; 
        }

        if (healItemPrefab != null && Random.Range(0f, 100f) <= healDropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }
    }
}