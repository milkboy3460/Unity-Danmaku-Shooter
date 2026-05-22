using UnityEngine;
using System.Collections;
using UnityEngine.Pool; // 追加

// エフェクトを一定時間後にプールへ戻して使い回すためのクラス。
// プールで再利用される前提なので、StartではなくOnEnableで毎回タイマーを動かす。
public class AutoDestroy : MonoBehaviour
{
    private IObjectPool<GameObject> _managedPool;
    private float lifeTime = 1.0f; // パーティクルが再生し終わるくらいまでの時間

    // どこに返すか（プール元）をセットしておく
    public void SetPool(IObjectPool<GameObject> pool)
    {
        _managedPool = pool;
    }

    // オブジェクトがアクティブ（使い回される状態）になるたびに呼ばれる
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