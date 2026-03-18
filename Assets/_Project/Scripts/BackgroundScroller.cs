using UnityEngine;

/// <summary>
/// 背景画像をループさせて無限スクロールを実現するクラス
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 3.0f;

    // 背景がシームレスに繋がる距離（配置した2枚目の画像のY座標と同じ値を指定する）
    [SerializeField] private float loopDistance = 58.3f;

    private Vector3 _startPosition;
    private float _offset;

    void Start()
    {
        // 毎フレームの計算基準とするため、初期位置をキャッシュしておく
        _startPosition = transform.position;
    }

    void Update()
    {
        // フレームレートに依存しないようにTime.deltaTimeを掛けて移動量を算出
        _offset += scrollSpeed * Time.deltaTime;

        // Mathf.RepeatでオフセットをloopDistanceの範囲内に収め、自然なループを作る
        float moveAmount = Mathf.Repeat(_offset, loopDistance);

        transform.position = _startPosition + Vector3.down * moveAmount;
    }
}