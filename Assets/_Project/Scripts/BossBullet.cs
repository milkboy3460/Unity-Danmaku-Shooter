using UnityEngine;

/// <summary>
/// ボスが発射する弾の基本挙動を制御するクラス。
/// 射角や速度は生成元（ボスのスクリプト）から設定される前提で直進する。
/// </summary>
public class BossBullet : MonoBehaviour
{
    [HideInInspector] public float speed = 5f;

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnBecameInvisible()
    {
        // 画面外に出た弾を自動破棄し、オブジェクトが無限に増え続けるのを防ぐ
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 実際のHP減算や被弾エフェクトなどのダメージ処理は被弾側（Player側）の責務とするため、
            // 弾のスクリプトでは「当たったら自身を消滅させる」処理のみを行う
            Destroy(gameObject); 
        }
    }
}