using UnityEngine;
using UnityEngine.Pool; // 追加

/// <summary>
/// ボスが発射する弾の基本挙動を制御するクラス。
/// オブジェクトプーリングに対応し、不要になった際はDestroyせずプールへ返却する。
/// </summary>
public class BossBullet : MonoBehaviour
{
    private IObjectPool<BossBullet> _managedPool;
    [HideInInspector] public float speed = 5f;

    /// <summary>
    /// プール管理元から自身の参照先を注入する。
    /// </summary>
    public void SetPool(IObjectPool<BossBullet> pool)
    {
        _managedPool = pool;
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnBecameInvisible()
    {
        ReleaseToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ReleaseToPool(); 
        }
    }

    /// <summary>
    /// 自身をプールへ返却する共通処理。
    /// </summary>
    private void ReleaseToPool()
    {
        if (_managedPool != null)
        {
            _managedPool.Release(this);
        }
        else
        {
            // プールを経由せずに生成された場合のフェイルセーフ
            Destroy(gameObject);
        }
    }
}