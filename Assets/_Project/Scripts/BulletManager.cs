using UnityEngine;
using UnityEngine.Pool;

// 弾と爆発エフェクトの「オブジェクトプール（使い回し）」をまとめて管理するクラス。
// 弾幕ゲーで毎回Instantiate/Destroyを繰り返すと、GC（ゴミ集め）が走って画面が一瞬止まる
// （GCスパイク）原因になるため、最初にまとめて作って再利用する方式をとっている。
public class BulletManager : MonoBehaviour
{
    // どこからでも呼べるようにSingletonにしておく
    public static BulletManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Bullet playerBulletPrefab;
    [SerializeField] private EnemyBullet enemyBulletPrefab;
    [SerializeField] private EnemyBullet midBossBulletPrefab; 
    [SerializeField] private BossBullet trueBossBulletPrefab;
    [SerializeField] private GameObject explosionPrefab;

    // 各プール。外部からも弾を借りれるようにプロパティにしておく
    public IObjectPool<Bullet> PlayerBulletPool { get; private set; }
    public IObjectPool<EnemyBullet> EnemyBulletPool { get; private set; }
    public IObjectPool<EnemyBullet> MidBossBulletPool { get; private set; } 
    public IObjectPool<BossBullet> TrueBossBulletPool { get; private set; }
    public IObjectPool<GameObject> ExplosionPool { get; private set; }

    private void Awake()
    {
        // シーン内にManagerが複数できないようにするお決まりの処理
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
        // 【自機弾プール】
        // プレイヤーの連射速度と画面に残る弾数を考慮して、初期50個・最大200個に設定。
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

        // 【ザコ敵弾プール】
        // ザコは複数同時に出るので少し多めに用意。
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

        // 【中ボス弾プール】
        // （ザコ弾と共有してもいいが、中ボスは弾の色を変える処理が入るため、
        // 　バグ防止と管理のしやすさのためにプールを分けておく）
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

        // 【真ボス弾プール】
        // 真ボスは狂ったような弾幕を張るので、最大1000個まで許容する。
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

        // 【爆発エフェクトプール】
        // 敵を倒したときに出るエフェクト。弾ほど大量には出ないので控えめに。
        ExplosionPool = new ObjectPool<GameObject>(
            createFunc: () => {
                var effect = Instantiate(explosionPrefab);
                var autoDestroy = effect.GetComponent<AutoDestroy>();
                // 自動で消える（プールに帰る）コンポーネントに、帰還先を教えておく
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

    // ===== ここから下は、他のクラスから弾を借りる（発射する）時に呼ぶメソッド群 =====

    public Bullet SpawnPlayerBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = PlayerBulletPool.Get();
        // 借りてきた弾の位置と角度を、発射口に合わせる
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