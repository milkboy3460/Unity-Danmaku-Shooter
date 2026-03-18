using UnityEngine;
using UnityEngine.Pool; // GCスパイク対策

/// <summary>
/// 角度と速度を指定して直進する汎用的な弾のコンポーネント。
/// オブジェクトプーリングに対応し、毎フレームの計算負荷を最小限に抑えた最適化モデル。
/// </summary>
public class Bullet : MonoBehaviour
{
    // 自身を管理しているオブジェクトプールへの参照
    private IObjectPool<Bullet> _managedPool;
    
    private float _speed;
    private Vector3 _velocity; // 毎フレームの三角関数計算を避けるため、進行ベクトルをキャッシュ
    private Camera _mainCamera; // Camera.mainのアクセスコストを削減するためのキャッシュ

    void Awake()
    {
        // Camera.mainは内部的にタグ検索(Find)が走るため、Awakeで一度だけ取得しキャッシュ（状態保持）する
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// プール管理元から自身の参照先を注入する。
    /// </summary>
    public void SetPool(IObjectPool<Bullet> pool)
    {
        _managedPool = pool;
    }

    /// <summary>
    /// 弾のパラメータを初期化する。
    /// </summary>
    /// <param name="speed">移動速度</param>
    /// <param name="angle">進行方向の角度（度数法）</param>
    public void Init(float speed, float angle)
    {
        _speed = speed;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 【最適化】Update内で毎フレームCos/Sinを計算するのはCPU負荷が高いため、
        // 発射時に一度だけ進行方向の単位ベクトルを計算してキャッシュしておく。
        float rad = angle * Mathf.Deg2Rad;
        _velocity = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
    }

    void Update()
    {
        // キャッシュされたベクトルを使うことで、フレーム単位の演算コストを極小化（等速直線運動）
        transform.position += _velocity * _speed * Time.deltaTime;

        CheckScreenOut();
    }

    /// <summary>
    /// 画面外判定を行い、不要になったオブジェクトをプールへ返却する。
    /// </summary>
    private void CheckScreenOut()
    {
        // キャッシュしたカメラを使用し、無駄な参照コストをカット
        Vector3 viewPos = _mainCamera.WorldToViewportPoint(transform.position);
        
        // 画面の境界ぴったりで消えると不自然なため、上下左右に0.1fのマージンを持たせる
        if (viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f)
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
            // プールを経由せずにInstantiateされた場合のフェイルセーフ
            Destroy(gameObject);
        }
    }
}