using UnityEngine;
using System.Collections;

/// <summary>
/// ゲームの最終目標である「真のボス」の挙動を制御するクラス。
/// 数百発の弾を吐き出すため、BulletManagerによるプーリングが必須となる要塞。
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
    public int ways = 6;             
    public float fireRate = 0.05f;   
    public float angleStep = 15f;    
    public float bulletSpeed = 5f;   
    public int burstCount = 20;      
    public float restTime = 2.0f;    
    private float currentAngle = 0f;
    private Coroutine attackCoroutine; 

    [Header("Defeat Sequence Settings")]
    [SerializeField] private GameObject finalExplosionPrefab; // 最終爆発専用プレハブ
    [SerializeField] private float defeatSequenceDuration = 2.5f; 
    [SerializeField] private float explosionInterval = 0.1f;      
    [SerializeField] private float finalExplosionScale = 4.0f;    
    
    private bool isDefeated = false; 

    void Start()
    {
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && spriteRenderer.sprite != null)
        {
            col.size = spriteRenderer.sprite.bounds.size * colliderScale;
        }

        transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    void Update()
    {
        if (isDefeated) return;

        if (!isReady)
        {
            transform.position += Vector3.down * enterSpeed * Time.deltaTime;
            if (transform.position.y <= stopPositionY)
            {
                transform.position = new Vector3(transform.position.x, stopPositionY, transform.position.z);
                isReady = true; 
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

    private IEnumerator SpiralAttackRoutine()
    {
        while (!isDefeated)
        {
            for (int j = 0; j < burstCount; j++)
            {
                if (isDefeated) yield break;

                if (BulletManager.Instance != null)
                {
                    for (int i = 0; i < ways; i++)
                    {
                        float angle = currentAngle + (360f / ways) * i;
                        Quaternion rotation = Quaternion.Euler(0, 0, angle + 180f); 
                        
                        // 変更点: Nway弾をすべてプールから展開し、GCを回避
                        BossBullet bulletScript = BulletManager.Instance.SpawnTrueBossBullet(transform.position, rotation);
                        bulletScript.speed = bulletSpeed;
                    }
                    currentAngle += angleStep;
                }
                
                if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayTrueBossShootSound();

                yield return new WaitForSeconds(fireRate); 
            }
            yield return new WaitForSeconds(restTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDefeated || !isReady) return;

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            // 自機弾の非アクティブ化
            collision.gameObject.SetActive(false);
            currentHp--;

            if (currentHp <= 0)
            {
                StartCoroutine(DefeatSequence());
            }
            else
            {
                if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayTrueBossDamageSound();
                StartCoroutine(DamageFlash());
            }
        }
    }

    private IEnumerator DefeatSequence()
    {
        isDefeated = true; 
        
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        Vector3 initialPosition = transform.position;
        float timer = 0f;
        Bounds bounds = spriteRenderer.bounds; 

        if (CameraShake.instance != null) CameraShake.instance.Shake(defeatSequenceDuration, 0.1f);

        // フェーズ1：ランダム誘爆（ここはプールを使う）
        while (timer < defeatSequenceDuration)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            
            if (BulletManager.Instance != null)
            {
                BulletManager.Instance.SpawnExplosion(randomPos, Quaternion.identity);
            }

            if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayExplosionSound();

            transform.position = initialPosition + (Vector3)Random.insideUnitCircle * 0.1f;
            spriteRenderer.color = (Random.value > 0.5f) ? Color.red : Color.white;

            timer += explosionInterval;
            yield return new WaitForSeconds(explosionInterval); 
        }

        // フェーズ2：最終爆発
        if (CameraShake.instance != null) CameraShake.instance.Shake(1.0f, 0.5f);

        // 注意: 最終爆発はスケールを巨大化させるため、プールを汚染しないよう個別にInstantiateする
        if (finalExplosionPrefab != null)
        {
            GameObject finalEx = Instantiate(finalExplosionPrefab, transform.position, Quaternion.identity);
            finalEx.transform.localScale *= finalExplosionScale;
        }

        if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayTrueBossExplosionSound();
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