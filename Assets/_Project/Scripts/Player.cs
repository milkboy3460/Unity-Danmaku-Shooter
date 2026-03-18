using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤー（自機）の移動、攻撃、ライフ管理を制御するメインクラス。
/// メモリ負荷を軽減するため、弾とエフェクトの生成・破棄は BulletManager を経由する設計に変更済。
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

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (hpDisplay != null) hpDisplay.UpdateHP(currentHP);
        
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

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, vertical, 0).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    private void HandleShooting()
    {
        fireTimer += Time.deltaTime; 

        if (Input.GetKey(KeyCode.Space) && fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f; 
        }
    }

    public void PowerUp()
    {
        fireRate -= powerUpAmount;
        if (fireRate < minFireRate) fireRate = minFireRate;
    }

    private void Shoot()
    {
        // 変更点: InstantiateとGetComponentを廃止し、GCアロケーション（メモリのゴミ）をゼロ化
        if (BulletManager.Instance != null)
        {
            Bullet b = BulletManager.Instance.SpawnPlayerBullet(transform.position, Quaternion.identity);
            b.Init(bulletSpeed, 90f);

            if (GameSoundManager.Instance != null)
            {
                GameSoundManager.Instance.PlayShootSound();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible || collision.gameObject.CompareTag("Item") || collision.gameObject.CompareTag("PowerUpItem")) return;

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("EnemyBullet"))
        {
            // 変更点: 敵弾はDestroyせず、非アクティブ化することでプールへ安全に返却させる
            if (collision.gameObject.CompareTag("EnemyBullet")) 
            {
                collision.gameObject.SetActive(false);
            }
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (hpDisplay != null) hpDisplay.UpdateHP(currentHP);

        if (CameraShake.instance != null) CameraShake.instance.Shake(0.3f, 0.2f);
        if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayDamageSound();

        if (currentHP <= 0) Die();
        else StartCoroutine(DamageRoutine());
    }

    public void Heal(int amount)
    {
        if (currentHP < maxHP)
        {
            currentHP += amount;
            if (currentHP > maxHP) currentHP = maxHP; 
            if (hpDisplay != null) hpDisplay.UpdateHP(currentHP);
        }
    }

    private IEnumerator DamageRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        isInvincible = false;
    }

    private void Die()
    {
        // 変更点: 爆発エフェクトもプールから取得して再利用
        if (BulletManager.Instance != null) 
        {
            BulletManager.Instance.SpawnExplosion(transform.position, Quaternion.identity);
        }

        if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayExplosionSound();

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.StartGameOverSequence(); 

        Destroy(gameObject);
    }
}