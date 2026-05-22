using UnityEngine;
using System.Collections;

// 画面揺らし（カメラシェイク）用のシングルトン。
// 被弾時やボス撃破時の「ヒット感」や「ダメージ感」を出してUX（手触り）を良くするためのもの。
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // 外部から「揺れる秒数」と「激しさ」を指定してカメラを揺らす
    public void Shake(float duration, float magnitude)
    {
        // 【バグ対策】
        // 連続でダメージを食らった時などに何度もShakeが呼ばれると、コルーチンが多重起動して
        // カメラがとんでもない勢いで吹っ飛んでいくため、実行中の揺れがあれば強制キャンセルする。
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        // 揺れ終わったあとにキッチリ元の位置に戻すため、開始時のローカル座標をメモっておく
        originalPos = transform.localPosition; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; 
        }

        // 【超重要】
        // これを忘れると、計算の誤差が蓄積してカメラがどんどん明後日の方向に
        // ズレていく（ドリフト現象）ので、最後は必ず元の位置に強制リセットする。
        transform.localPosition = originalPos;
    }
}