using UnityEngine;
using System.Collections;

/// <summary>
/// 画面揺れ（カメラシェイク）演出を管理するシングルトンクラス。
/// ボス撃破時や被弾時などの視覚的フィードバックとして使用する。
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    /// <summary>
    /// カメラシェイクを実行する。
    /// </summary>
    /// <param name="duration">揺れる時間（秒）</param>
    /// <param name="magnitude">揺れの振幅（激しさ）</param>
    public void Shake(float duration, float magnitude)
    {
        // 連続で呼ばれた際、コルーチンが重複して不自然な挙動になるのを防ぐ
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        // 揺れ終わった後に正確な位置へ戻すため、開始時のローカル座標をキャッシュしておく
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

        // カメラの座標ズレ（ドリフト）を確実に防ぐため、ループ終了後に必ず元の位置にリセットする
        transform.localPosition = originalPos;
    }
}