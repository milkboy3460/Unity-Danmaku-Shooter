using UnityEngine;
using System.Collections;

/// <summary>
/// ゲームの最終目標である「真のボス」の挙動を制御するクラス。
/// 高密度の螺旋弾幕アルゴリズムと、撃破時の動的な演出シーケンスを搭載する。
/// </summary>
public class TrueBoss : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private int maxHp = 100; 
    private int currentHp;
    [SerializeField] private int scoreValue = 20000;

    [Header("Visual & Physics")]
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float colliderScale = 0.6f;

    [Header("Movement Settings")]
    [SerializeField] private float stopPositionY = 3.5f; 
    [SerializeField] private float enterSpeed = 1.5f;    
    [SerializeField] private float moveSpeedX = 2.0f;
    [SerializeField] private float leftLimit = -3.0f;
    [SerializeField] private float rightLimit = 3.0f;
    private bool isReady = false;
    private int direction = 1;

    [Header("Bullet Hell Settings")]
    [SerializeField] private GameObject trueBossBulletPrefab; 
    public int ways = 6;             // 同時発射数（N-way弾）
    public float fireRate = 0.05f;   // 発射間隔（弾幕の密度）
    public float angleStep = 15f;    // 1射ごとの射角増分
    public float bulletSpeed = 5f;   
    public int burstCount = 20;      // 1サイクルあたりの連射数
    public float restTime = 2.0f;    // サイクル間の待機時間
    private float currentAngle = 0f;
    private Coroutine attackCoroutine; 

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    
    [Header("Defeat Sequence Settings")]
    [SerializeField] private float defeatSequenceDuration = 2.5f; // 演出の総継続時間
    [SerializeField] private float explosionInterval = 0.1f;      // 誘爆の間隔
    [SerializeField] private float finalExplosionScale = 4.0f;    // 最終爆発の規模
    
    private bool isDefeated = false; 

    void Start()
    {
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && spriteRenderer.sprite != null)
        {
            // スプライトの境界（Bounds）に基づき、当たり判定を適切なサイズに自動調整
            col.size = spriteRenderer.sprite.bounds.size * colliderScale;
        }

        transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    void Update()
    {
        if (isDefeated) return;

        if (!isReady)
        {
            // 所定の位置まで降下する登場フェーズ
            transform.position += Vector3.down * enterSpeed * Time.deltaTime;
            if (transform.position.y <= stopPositionY)
            {
                transform.position = new Vector3(transform.position.x, stopPositionY, transform.position.z);
                isReady = true; 
                // 待機位置到達と同時に攻撃ルーチンを開始
                attackCoroutine = StartCoroutine(SpiralAttackRoutine());
            }
        }
        else
        {
            MoveAction();
        }
    }

    private void MoveAction()
    {
        transform.position += Vector3.right * direction * moveSpeedX * Time.deltaTime;
        if (transform.position.x >= rightLimit) direction = -1;
        else if (transform.position.x <= leftLimit) direction = 1;
    }

    /// <summary>
    /// 回転しながら全方位に弾を放つ螺旋弾幕のコルーチン。
    /// fireRate と angleStep の調整により、弾幕の密度と回転速度を制御可能。
    /// </summary>
    private IEnumerator SpiralAttackRoutine()
    {
        while (!isDefeated)
        {
            for (int j = 0; j < burstCount; j++)
            {
                if (isDefeated) yield break;

                if (trueBossBulletPrefab != null)
                {
                    for (int i = 0; i < ways; i++)
                    {
                        // 円周を ways で均等分割し、基準角 currentAngle を加算して回転させる
                        float angle = currentAngle + (360f / ways) * i;
                        Quaternion rotation = Quaternion.Euler(0, 0, angle + 180f); 
                        
                        GameObject bullet = Instantiate(trueBossBulletPrefab, transform.position, rotation);
                        
                        BossBullet bulletScript = bullet.GetComponent<BossBullet>();
                        if (bulletScript != null)
                        {
                            bulletScript.speed = bulletSpeed;
                        }
                    }
                    // 次の射撃に向けて角度を更新（螺旋のひねりを生む）
                    currentAngle += angleStep;
                }
                
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.PlayTrueBossShootSound();
                }

                yield return new WaitForSeconds(fireRate); 
            }

            // 1バースト終了後のインターバル（プレイヤーの回避スペースを確保）
            yield return new WaitForSeconds(restTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDefeated || !isReady) return;

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            currentHp--;

            if (currentHp <= 0)
            {
                StartCoroutine(DefeatSequence());
            }
            else
            {
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.PlayTrueBossDamageSound();
                }
                StartCoroutine(DamageFlash());
            }
        }
    }

    /// <summary>
    /// 撃破時の特別演出シーケンス。
    /// 連続した誘爆、機体の振動、色の明滅を経て、最終的な大爆発へと繋げる。
    /// </summary>
    private IEnumerator DefeatSequence()
    {
        isDefeated = true; 
        
        // 演出中に不要な判定が走らないよう当たり判定を無効化
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;

        // 攻撃コルーチンを即座に停止
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        Vector3 initialPosition = transform.position;
        float timer = 0f;
        Bounds bounds = spriteRenderer.bounds; 

        // 演出開始のインパクトを与えるシェイク
        if (CameraShake.instance != null) CameraShake.instance.Shake(defeatSequenceDuration, 0.1f);

        // フェーズ1：機体範囲内でのランダムな誘爆演出
        while (timer < defeatSequenceDuration)
        {
            if (explosionPrefab != null)
            {
                Vector2 randomPos = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
                
                Instantiate(explosionPrefab, randomPos, Quaternion.identity);
            }

            if (GameSoundManager.Instance != null)
            {
                GameSoundManager.Instance.PlayExplosionSound();
            }

            // 破壊の衝撃を表現するランダムな座標微変動（ジリジリとした揺れ）
            transform.position = initialPosition + (Vector3)Random.insideUnitCircle * 0.1f;
            spriteRenderer.color = (Random.value > 0.5f) ? Color.red : Color.white;

            timer += explosionInterval;
            yield return new WaitForSeconds(explosionInterval); 
        }

        // フェーズ2：最終爆発（フィナーレ）
        if (CameraShake.instance != null) CameraShake.instance.Shake(1.0f, 0.5f);

        if (explosionPrefab != null)
        {
            GameObject finalEx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            finalEx.transform.localScale *= finalExplosionScale;
        }

        if (GameSoundManager.Instance != null)
        {
            GameSoundManager.Instance.PlayTrueBossExplosionSound();
        }

        if (ScoreManager.instance != null) ScoreManager.instance.AddScore(scoreValue);

        Destroy(gameObject);
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null && !isDefeated)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }
}