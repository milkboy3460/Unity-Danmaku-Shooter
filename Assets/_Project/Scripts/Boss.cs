using UnityEngine;
using System.Collections;

/// <summary>
/// 中ボスの挙動を制御するクラス。
/// 生成時にランダムに選ばれた属性（色）に応じて、異なる弾幕パターン（Nway, 螺旋, 追尾）を展開する。
/// </summary>
public class Boss : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private int maxHp = 30;
    private int currentHp;
    [SerializeField] private int scoreValue = 5000;

    [Header("Appearance")]
    [SerializeField] private Sprite[] bossSprites; // 0:Red(Nway), 1:Green(Spiral), 2:Blue(Homing)
    private SpriteRenderer spriteRenderer;
    private int bossType = 0; 

    [Header("Movement Settings")]
    [SerializeField] private float colliderScale = 0.7f;
    [SerializeField] private float stopPositionY = 4.5f;
    [SerializeField] private float enterSpeed = 2.0f;
    [SerializeField] private float moveSpeedX = 3.0f;
    [SerializeField] private float leftLimit = -2.5f;
    [SerializeField] private float rightLimit = 2.5f;
    private bool isReady = false;
    private int direction = 1;

    [Header("Attack Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float fireRate = 1.2f;
    private float fireTimer = 0f;
    private float spiralAngle = 0f; 

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;

    void Start()
    {
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (bossSprites != null && bossSprites.Length > 0)
        {
            bossType = Random.Range(0, bossSprites.Length);
            spriteRenderer.sprite = bossSprites[bossType];
            
            // スプライトの大きさに合わせて当たり判定を動的に自動調整する
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = spriteRenderer.sprite.bounds.size * colliderScale;
            }
        }

        transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    void Update()
    {
        if (!isReady)
        {
            // 初期位置まで降下するフェーズ
            transform.position += Vector3.down * enterSpeed * Time.deltaTime;
            if (transform.position.y <= stopPositionY)
            {
                transform.position = new Vector3(transform.position.x, stopPositionY, transform.position.z);
                isReady = true; 
            }
        }
        else
        {
            MoveAction();
            
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                AttackByColor();
                fireTimer = 0f;
            }
        }
    }

    private void MoveAction()
    {
        transform.position += Vector3.right * direction * moveSpeedX * Time.deltaTime;
        if (transform.position.x >= rightLimit) direction = -1;
        else if (transform.position.x <= leftLimit) direction = 1;
    }

    private void AttackByColor()
    {
        if (enemyBulletPrefab == null) return;

        // 生成時に決定した属性に基づいて弾幕アルゴリズムを分岐
        switch (bossType)
        {
            case 0: ShootSpread(3, 30f); break; 
            case 1: ShootSpiral(); break;       
            case 2: ShootHoming(); break;       
        }

        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.PlayMidBossShootSound();
        }
    }

    /// <summary>
    /// Nway弾（扇状の拡散弾）を生成する。
    /// 指定された総角度(angleRange)を弾数で等分し、放射状のベクトルを計算する。
    /// </summary>
    private void ShootSpread(int count, float angleRange)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = -(angleRange / 2) + (angleRange / (count - 1)) * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle + 180f);
            GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, rotation);
            
            EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
            if (eb != null)
            {
                eb.SetBulletSprite(0); 
                eb.SetDirection(rotation * Vector3.up);
            }
        }
    }

    /// <summary>
    /// 螺旋弾（渦巻き弾）を生成する。
    /// 発射のたびに基準角度(spiralAngle)をずらし、回転するような軌道を描かせる。
    /// </summary>
    private void ShootSpiral()
    {
        // 180度対称の2方向から同時に螺旋を発射する
        for (int i = 0; i < 2; i++)
        {
            float angle = spiralAngle + (i * 180f);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, rotation);
            
            EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
            if (eb != null)
            {
                eb.SetBulletSprite(1); 
                eb.SetDirection(rotation * Vector3.up);
            }
        }
        // 次回の発射に向けて基準角度を更新（この数値が渦の密度に影響する）
        spiralAngle += 20f;
    }

    /// <summary>
    /// 自機狙い弾（ホーミング弾）を生成する。
    /// 発射時点でのプレイヤーの座標を取得し、その方向ベクトル(Atan2)へ向けて射出する。
    /// </summary>
    private void ShootHoming()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 targetDir = Vector3.down;
        
        if (player != null)
        {
            targetDir = (player.transform.position - transform.position).normalized;
        }

        // 方向ベクトルからZ軸の回転角度を算出
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f); 

        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, rotation);
        
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.SetBulletSprite(2); 
            eb.SetDirection(targetDir);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            currentHp--;

            if (currentHp <= 0)
            {
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.PlayMidBossExplosionSound();
                }

                if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                if (ScoreManager.instance != null) ScoreManager.instance.AddScore(scoreValue);

                Destroy(gameObject);
            }
            else
            {
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.PlayMidBossDamageSound();
                }
                // ダメージ時の視覚的フィードバック
                StartCoroutine(DamageFlash());
            }
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }
}