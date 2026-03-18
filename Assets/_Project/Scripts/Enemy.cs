using UnityEngine;

/// <summary>
/// 一般的な敵キャラクターの挙動（移動・攻撃・被弾・ドロップ）を管理するクラス。
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float fireRate = 2.0f;
    private float fireTimer = 0f;

    [Header("Score & Effects")]
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Drop Item Settings")]
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
        if (enemyBulletPrefab != null)
        {
            Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddScore(scoreValue);
            }

            DropItem();

            // 自身（Enemyオブジェクト）はこの直後に破棄されるため、
            // 音切れを防ぐ目的で永続的に存在するSoundManagerへ再生処理を委譲する
            if (GameSoundManager.Instance != null)
            {
                GameSoundManager.Instance.PlayExplosionSound();
            }

            Destroy(gameObject); 
        }
    }

    /// <summary>
    /// 撃破時のアイテムドロップ判定。
    /// 複数のアイテムが同時にドロップするのを防ぐため、排他的な確率抽選を行う。
    /// </summary>
    private void DropItem()
    {
        // 希少価値の高いパワーアップアイテムから優先して抽選
        if (powerUpItemPrefab != null && Random.Range(0f, 100f) <= powerUpDropChance)
        {
            Instantiate(powerUpItemPrefab, transform.position, Quaternion.identity);
            
            // 当選した場合は早期リターン（Early Return）し、以降の抽選をスキップする
            return; 
        }

        // パワーアップの抽選に漏れた場合のみ、回復アイテムの抽選を行う（フォールバック）
        if (healItemPrefab != null && Random.Range(0f, 100f) <= healDropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }
    }
}