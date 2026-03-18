using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 弾とエフェクトのオブジェクトプールを一括管理するクラス。
/// Instantiate/DestroyによるGCスパイクを防ぎ、パフォーマンスを安定させる目的で実装しています。
/// </summary>
public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Bullet playerBulletPrefab;
    [SerializeField] private EnemyBullet enemyBulletPrefab;
    [SerializeField] private EnemyBullet midBossBulletPrefab; 
    [SerializeField] private BossBullet trueBossBulletPrefab;
    [SerializeField] private GameObject explosionPrefab;

    // 外部からアクセスできるようプロパティ化
    public IObjectPool<Bullet> PlayerBulletPool { get; private set; }
    public IObjectPool<EnemyBullet> EnemyBulletPool { get; private set; }
    public IObjectPool<EnemyBullet> MidBossBulletPool { get; private set; } 
    public IObjectPool<BossBullet> TrueBossBulletPool { get; private set; }
    public IObjectPool<GameObject> ExplosionPool { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        // 自機弾
        PlayerBulletPool = new ObjectPool<Bullet>(
            createFunc: () => {
                var bullet = Instantiate(playerBulletPrefab);
                bullet.SetPool(PlayerBulletPool); 
                return bullet;
            },
            actionOnGet: (bullet) => bullet.gameObject.SetActive(true),
            actionOnRelease: (bullet) => bullet.gameObject.SetActive(false),
            actionOnDestroy: (bullet) => Destroy(bullet.gameObject),
            collectionCheck: false, defaultCapacity: 50, maxSize: 200
        );

        // ザコ敵弾
        EnemyBulletPool = new ObjectPool<EnemyBullet>(
            createFunc: () => {
                var bullet = Instantiate(enemyBulletPrefab);
                bullet.SetPool(EnemyBulletPool);
                return bullet;
            },
            actionOnGet: (bullet) => bullet.gameObject.SetActive(true),
            actionOnRelease: (bullet) => bullet.gameObject.SetActive(false),
            actionOnDestroy: (bullet) => Destroy(bullet.gameObject),
            collectionCheck: false, defaultCapacity: 100, maxSize: 500
        );

        // 中ボス弾（3色のスプライト切り替えを行うためザコと分離）
        MidBossBulletPool = new ObjectPool<EnemyBullet>(
            createFunc: () => {
                var bullet = Instantiate(midBossBulletPrefab);
                bullet.SetPool(MidBossBulletPool);
                return bullet;
            },
            actionOnGet: (bullet) => bullet.gameObject.SetActive(true),
            actionOnRelease: (bullet) => bullet.gameObject.SetActive(false),
            actionOnDestroy: (bullet) => Destroy(bullet.gameObject),
            collectionCheck: false, defaultCapacity: 100, maxSize: 500
        );

        // 真ボス弾
        TrueBossBulletPool = new ObjectPool<BossBullet>(
            createFunc: () => {
                var bullet = Instantiate(trueBossBulletPrefab);
                bullet.SetPool(TrueBossBulletPool);
                return bullet;
            },
            actionOnGet: (bullet) => bullet.gameObject.SetActive(true),
            actionOnRelease: (bullet) => bullet.gameObject.SetActive(false),
            actionOnDestroy: (bullet) => Destroy(bullet.gameObject),
            collectionCheck: false, defaultCapacity: 200, maxSize: 1000
        );

        // 爆発エフェクト
        ExplosionPool = new ObjectPool<GameObject>(
            createFunc: () => {
                var effect = Instantiate(explosionPrefab);
                var autoDestroy = effect.GetComponent<AutoDestroy>();
                if (autoDestroy != null) autoDestroy.SetPool(ExplosionPool); 
                return effect;
            },
            actionOnGet: (effect) => effect.SetActive(true),
            actionOnRelease: (effect) => effect.SetActive(false),
            actionOnDestroy: (effect) => Destroy(effect),
            collectionCheck: false, defaultCapacity: 30, maxSize: 100
        );
    }

    #region Spawn APIs

    public Bullet SpawnPlayerBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = PlayerBulletPool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);
        return bullet;
    }

    public EnemyBullet SpawnEnemyBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = EnemyBulletPool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);
        return bullet;
    }

    public EnemyBullet SpawnMidBossBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = MidBossBulletPool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);
        return bullet;
    }

    public BossBullet SpawnTrueBossBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = TrueBossBulletPool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);
        return bullet;
    }

    public GameObject SpawnExplosion(Vector3 position, Quaternion rotation)
    {
        var effect = ExplosionPool.Get();
        effect.transform.SetPositionAndRotation(position, rotation);
        return effect;
    }
    #endregion
}