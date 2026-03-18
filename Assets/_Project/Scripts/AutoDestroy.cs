using UnityEngine;

/// <summary>
/// 生成されたエフェクトを一定時間後に自動破棄し、メモリを解放するクラス
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        
        // パーティクルの再生時間（Duration + StartLifetime）を考慮し、
        // 演出が完全に終わるよう安全マージンを取って1.0秒後に破棄する
        Destroy(gameObject, 1.0f);
    }
}