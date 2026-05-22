using UnityEngine;
using UnityEngine.Pool; // 追加

// ボスの弾の動きを制御するクラス。
// 弾幕で大量に出るので、いちいちDestroyせずにオブジェクトプールに返して使い回す。
public class BossBullet : MonoBehaviour
{
    private IObjectPool<BossBullet> _managedPool;
    [HideInInspector] public float speed = 5f;

    // 自分がどこのプールに帰ればいいか（管理元）をセットしておく
    public void SetPool(IObjectPool<BossBullet> pool)
    {
        _managedPool = pool;
    }

    void Update()
    {
        // ローカルのY軸方向（前）に進み続ける。発射角度は生成元で合わせる想定。
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnBecameInvisible()
    {
        // カメラに映らなくなった（画面外に出た）瞬間に、不要になるのでプールに回収する
        ReleaseToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーに当たった時も弾を消す（プールに戻す）
        if (collision.gameObject.CompareTag("Player"))
        {
            ReleaseToPool(); 
        }
    }

    // プールへ戻す共通処理
    private void ReleaseToPool()
    {
        if (_managedPool != null)
        {
            _managedPool.Release(this);
        }
        else
        {
            // デバッグ等でプールを使わずに直接Instantiateされた時のための保険
            Destroy(gameObject);
        }
    }
}