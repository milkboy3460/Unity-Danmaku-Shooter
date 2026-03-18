using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤー（自機）の移動、攻撃、ライフ管理を制御するメインクラス。
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float minX = -8.5f;
    [SerializeField] private float maxX = 8.5f;
    [SerializeField] private float minY = -4.8f;
    [SerializeField] private float maxY = 4.8f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.5f;         
    [SerializeField] private float minFireRate = 0.1f;      
    [SerializeField] private float powerUpAmount = 0.1f;    
    [SerializeField] private float bulletSpeed = 10.0f;     
    
    private float fireTimer = 0f;

    [Header("HP & Damage Settings")]
    [SerializeField] private int maxHP = 3;
    private int currentHP;
    [SerializeField] private HPDisplay hpDisplay; 
    [SerializeField] private float invincibleTime = 1.5f;
    private bool isInvincible = false;

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初期HPをUIに反映
        if (hpDisplay != null) 
        {
            hpDisplay.UpdateHP(currentHP);
        }
        
        // パラメータの初期化（インスペクター設定値の上書き）
        fireRate = 0.5f;
        minFireRate = 0.1f;
        powerUpAmount = 0.1f;
        
        fireTimer = fireRate;
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    /// <summary>
    /// 入力に基づいた移動処理と、移動範囲の制限（Clamp）を行う。
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 斜め移動が速くならないよう正規化して移動量を計算
        Vector3 direction = new Vector3(horizontal, vertical, 0).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 画面外へ出ないように座標を制限
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    /// <summary>
    /// 攻撃入力の受付と、連射速度（fireRate）に基づいた発射制限を行う。
    /// </summary>
    private void HandleShooting()
    {
        fireTimer += Time.deltaTime; 

        if (Input.GetKey(KeyCode.Space) && fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f; 
        }
    }

    /// <summary>
    /// アイテム取得時などのパワーアップ処理。連射性能を向上させる。
    /// </summary>
    public void PowerUp()
    {
        fireRate -= powerUpAmount;
        if (fireRate < minFireRate) fireRate = minFireRate;
        Debug.Log($"Power Up: Current Fire Rate = {fireRate}s");
    }

    private void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject obj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            
            Bullet b = obj.GetComponent<Bullet>();
            if (b != null)
            {
                // 弾丸の進行方向（上方向90度）と初速を設定
                b.Init(bulletSpeed, 90f);
            }

            if (GameSoundManager.Instance != null)
            {
                GameSoundManager.Instance.PlayShootSound();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 無敵時間中、またはアイテム系との衝突時はダメージ判定をスキップ
        if (isInvincible || collision.gameObject.CompareTag("Item") || collision.gameObject.CompareTag("PowerUpItem")) return;

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("EnemyBullet"))
        {
            // 敵弾との衝突時は弾側を消滅させる
            if (collision.gameObject.CompareTag("EnemyBullet")) Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    /// <summary>
    /// ダメージを受け、ライフの減算と被弾演出を開始する。
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (hpDisplay != null) hpDisplay.UpdateHP(currentHP);

        // 被弾時のインパクトを強調するためのカメラシェイク演出
        if (CameraShake.instance != null) CameraShake.instance.Shake(0.3f, 0.2f);

        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.PlayDamageSound();
        }

        if (currentHP <= 0) Die();
        else StartCoroutine(DamageRoutine());
    }

    /// <summary>
    /// ライフを回復させる。最大値を超えないよう制御する。
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHP < maxHP)
        {
            currentHP += amount;
            if (currentHP > maxHP) currentHP = maxHP; 
            if (hpDisplay != null) hpDisplay.UpdateHP(currentHP);
        }
    }

    /// <summary>
    /// 被弾後の無敵時間演出。スプライトを点滅させる。
    /// </summary>
    private IEnumerator DamageRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            // アルファ値の切り替えによる点滅表現
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        isInvincible = false;
    }

    /// <summary>
    /// 死亡時の処理。爆発演出の生成とゲームオーバーシーケンスの開始を行う。
    /// </summary>
    private void Die()
    {
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.PlayExplosionSound();
        }

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.StartGameOverSequence(); 

        Destroy(gameObject);
    }
}