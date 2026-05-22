using UnityEngine;
using System.Collections;

// 中ボスのコントロール用クラス。
// 出現時に色（赤・緑・青）をランダムで決めて、色ごとに違うパターンの弾幕を撃たせる。
// ※弾や爆発エフェクトを毎回Instantiate/Destroyするとガベージコレクションが走ってカクつく原因になるため、
// 生成処理はBulletManager（オブジェクトプール）に任せている。
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
            
            // 選ばれたスプライト（画像）の大きさに合わせて、当たり判定のサイズも自動調整する
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null) col.size = spriteRenderer.sprite.bounds.size * colliderScale;
        }

        transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    void Update()
    {
        if (!isReady)
        {
            // まずは画面上部から指定の高さまで降りてくる（入場演出）
            transform.position += Vector3.down * enterSpeed * Time.deltaTime;
            if (transform.position.y <= stopPositionY)
            {
                transform.position = new Vector3(transform.position.x, stopPositionY, transform.position.z);
                isReady = true; 
            }
        }
        else
        {
            // 定位置に着いたら、左右への移動と攻撃をスタートする
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
        // 画面の端から端を反復横飛びさせる
        transform.position += Vector3.right * direction * moveSpeedX * Time.deltaTime;
        if (transform.position.x >= rightLimit) direction = -1;
        else if (transform.position.x <= leftLimit) direction = 1;
    }

    private void AttackByColor()
    {
        if (BulletManager.Instance == null) return;

        switch (bossType)
        {
            case 0: ShootSpread(3, 30f); break; 
            case 1: ShootSpiral(); break;       
            case 2: ShootHoming(); break;       
        }

        if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayMidBossShootSound();
    }

    private void ShootSpread(int count, float angleRange)
    {
        for (int i = 0; i < count; i++)
        {
            // 扇状に弾を綺麗に散らすための角度計算
            float angle = -(angleRange / 2) + (angleRange / (count - 1)) * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle + 180f);
            
            // ★弾を新規作成せず、中ボス専用のプールから引っ張ってくる（負荷対策）
            EnemyBullet eb = BulletManager.Instance.SpawnMidBossBullet(transform.position, rotation);
            if (eb != null)
            {
                eb.SetBulletSprite(0); 
                eb.SetDirection(rotation * Vector3.up);
            }
        }
    }

    private void ShootSpiral()
    {
        for (int i = 0; i < 2; i++)
        {
            float angle = spiralAngle + (i * 180f);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            // ★ここもプールから取得
            EnemyBullet eb = BulletManager.Instance.SpawnMidBossBullet(transform.position, rotation);
            if (eb != null)
            {
                eb.SetBulletSprite(1); 
                eb.SetDirection(rotation * Vector3.up);
            }
        }
        // 撃つたびに基準の角度をズラすことで、渦巻き状の弾幕にする
        spiralAngle += 20f;
    }

    private void ShootHoming()
    {
        // 撃つ瞬間のプレイヤーの位置を計算して狙い撃つ
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 targetDir = Vector3.down;
        
        if (player != null) targetDir = (player.transform.position - transform.position).normalized;

        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f); 

        // ★ここもプールから取得
        EnemyBullet eb = BulletManager.Instance.SpawnMidBossBullet(transform.position, rotation);
        if (eb != null)
        {
            eb.SetBulletSprite(2); 
            eb.SetDirection(targetDir);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーの弾が当たった時の処理
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            collision.gameObject.SetActive(false);
            currentHp--;

            if (currentHp <= 0)
            {
                if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayMidBossExplosionSound();

                // 倒した時。爆発エフェクトも生成(Instantiate)すると重いのでプールから借りる
                if (BulletManager.Instance != null) BulletManager.Instance.SpawnExplosion(transform.position, Quaternion.identity);
                if (ScoreManager.instance != null) ScoreManager.instance.AddScore(scoreValue);

                Destroy(gameObject);
            }
            else
            {
                if (GameSoundManager.Instance != null) GameSoundManager.Instance.PlayMidBossDamageSound();
                // ダメージが入っていることが視覚的に分かるように赤く光らせる
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