using UnityEngine;

// 背景画像を無限スクロール（ループ）させるためのクラス
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 3.0f;

    // 画像がループする距離（2枚目の画像のY座標と同じ値を入れる）
    [SerializeField] private float loopDistance = 58.3f;

    private Vector3 _startPosition;
    private float _offset;

    void Start()
    {
        // ループ計算の基準にするため、最初の位置を覚えておく
        _startPosition = transform.position;
    }

    void Update()
    {
        // 下に向かって移動させる距離を足していく
        _offset += scrollSpeed * Time.deltaTime;

        // 一定の距離（loopDistance）まで進んだら、オフセットを0に戻してループさせる
        float moveAmount = Mathf.Repeat(_offset, loopDistance);

        transform.position = _startPosition + Vector3.down * moveAmount;
    }
}