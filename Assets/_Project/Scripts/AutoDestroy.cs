using UnityEngine;
using System.Collections;
using UnityEngine.Pool; // 追加

/// <summary>
/// 生成されたエフェクトを一定時間後にプールへ自動返却し、メモリを解放するクラス。
/// プーリング環境下での再利用を前提に、StartではなくOnEnableでタイマーを管理する。
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    private IObjectPool<GameObject> _managedPool;
    private float lifeTime = 1.0f; // パーティクルの再生時間をカバーする安全マージン

    /// <summary>
    /// プール管理元から自身の参照先を注入する。
    /// </summary>
    public void SetPool(IObjectPool<GameObject> pool)
    {
        _managedPool = pool;
    }

    // 使い回されるたびに（SetActive(true)になるたびに）呼ばれる
    void OnEnable()
    {
        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        ReleaseToPool();
    }

    private void ReleaseToPool()
    {
        if (_managedPool != null)
        {
            _managedPool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}