using UnityEngine;

/// <summary>
/// 角度と速度を指定して直進する汎用的な弾のコンポーネント。
/// </summary>
public class Bullet : MonoBehaviour
{
    private float _speed;
    private float _angle;

    /// <summary>
    /// 弾のパラメータを初期化する。
    /// Instantiate直後にStart()関数の実行を待たずに確実に値を確定させるため、明示的なメソッドを採用。
    /// </summary>
    /// <param name="speed">移動速度</param>
    /// <param name="angle">進行方向の角度（度数法）</param>
    public void Init(float speed, float angle)
    {
        _speed = speed;
        _angle = angle;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // 角度（Degree）をラジアン（Radian）に変換し、三角関数を用いて進行ベクトルを算出
        float rad = _angle * Mathf.Deg2Rad;
        Vector3 velocity = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
        
        transform.position += velocity * _speed * Time.deltaTime;

        CheckScreenOut();
    }

    /// <summary>
    /// 画面外判定を行い、不要になったオブジェクトを破棄（またはプールへ返却）する。
    /// </summary>
    private void CheckScreenOut()
    {
        // カメラのビューポート座標（0.0〜1.0）を基準に画面外に出たかを判定
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        
        // 画面の境界ぴったりで消えるとプレイヤーから見て不自然なため、上下左右に0.1fのマージン（遊び）を持たせている
        if (viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }
}