using UnityEngine;
using UnityEngine.Pool; // カクつき（GCスパイク）対策

// 汎用的な弾のクラス。
// 弾幕シューティングでは画面上に大量の弾が出るため、毎フレームの計算を極限まで削って軽くしている。
public class Bullet : MonoBehaviour
{
    // 使い終わったあとに帰るプール
    private IObjectPool<Bullet> _managedPool;
    
    private float _speed;
    private Vector3 _velocity; // 毎フレームの計算をサボるための進行方向キャッシュ
    private Camera _mainCamera; 

    void Awake()
    {
        // Camera.mainは呼ぶたびに裏でタグ検索(Find)が走って激重なので、
        // 起動時に1回だけ取得して変数に持っておく
        _mainCamera = Camera.main;
    }

    // どこに返すか（プール元）をセットする
    public void SetPool(IObjectPool<Bullet> pool)
    {
        _managedPool = pool;
    }

    // 弾を発射する時の初期設定
    public void Init(float speed, float angle)
    {
        _speed = speed;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 【負荷対策】
        // 弾が飛ぶたびにUpdateの中で毎回CosやSinを計算するとCPU負荷が跳ね上がる。
        // なので「発射された瞬間」に1回だけ計算して、進む方向のベクトルを作っておく。
        float rad = angle * Mathf.Deg2Rad;
        _velocity = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
    }

    void Update()
    {
        // 上で計算しておいたベクトルを足すだけ。これでフレーム単位の処理が劇的に軽くなる。
        transform.position += _velocity * _speed * Time.deltaTime;

        CheckScreenOut();
    }

    // 画面外に出たらプールに返す処理
    private void CheckScreenOut()
    {
        // ここでも上でキャッシュしたカメラを使って無駄な処理を省く
        Vector3 viewPos = _mainCamera.WorldToViewportPoint(transform.position);
        
        // 画面の境界線(0や1)ぴったりで消すと、プレイヤーから「突然弾が消滅した」ように見えて不自然。
        // なので、上下左右に0.1fずつマージン（余裕）を持たせて完全に画面外に出てから消す。
        if (viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f)
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
            // プールを使わずに直接Instantiateされた時のための保険
            Destroy(gameObject);
        }
    }
}