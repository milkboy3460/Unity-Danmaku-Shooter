using System.Collections;
using UnityEngine;

/// <summary>
/// 敵の編隊（フォーメーション）を一定間隔で生成するウェーブ管理クラス。
/// </summary>
public class FormationSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefabA; // 直進タイプ
    [SerializeField] private GameObject enemyPrefabB; // 斜め移動タイプ
    [SerializeField] private GameObject enemyPrefabC; // 波状移動（サインカーブ）タイプ

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 4.0f; 
    [SerializeField] private float spawnY = 9.0f;        // 画面外上部の基準Y座標

    void Start()
    {
        // 敵のスポーンサイクル（メインループ）を非同期で開始
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 設定されたインターバルに従い、ランダムなフォーメーションを展開し続けるメインルーチン。
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        while (true) 
        {
            int pattern = Random.Range(0, 3);

            switch (pattern)
            {
                case 0:
                    SpawnLineFormation();
                    break;
                case 1:
                    SpawnVFormation();
                    break;
                case 2:
                    SpawnDiagonalFormation();
                    break;
            }

            // メインスレッドをブロックすることなく、指定秒数待機して次のウェーブへ移行
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    #region Formation Patterns

    /// <summary>
    /// パターンA：横一列のフォーメーションを展開する。
    /// </summary>
    private void SpawnLineFormation()
    {
        float[] xPositions = { -2.5f, 0f, 2.5f }; 
        
        foreach (float x in xPositions)
        {
            Vector3 pos = new Vector3(x, spawnY, 0);
            Instantiate(enemyPrefabA, pos, Quaternion.identity);
        }
    }

    /// <summary>
    /// パターンB：V字型のフォーメーションを展開する。
    /// Y座標のオフセットを利用することで、各機体の画面進入タイミングに時間差を生じさせる。
    /// </summary>
    private void SpawnVFormation()
    {
        Vector2[] positions = {
            new Vector2(0, 0),         // 先頭（中央）
            new Vector2(-1.5f, 1.5f),  // 左翼・前
            new Vector2(1.5f, 1.5f),   // 右翼・前
            new Vector2(-3.0f, 3.0f),  // 左翼・後
            new Vector2(3.0f, 3.0f)    // 右翼・後
        };

        foreach (Vector2 offset in positions)
        {
            Vector3 pos = new Vector3(offset.x, spawnY + offset.y, 0);
            Instantiate(enemyPrefabC, pos, Quaternion.identity);
        }
    }

    /// <summary>
    /// パターンC：斜め一列のフォーメーションを展開する。
    /// </summary>
    private void SpawnDiagonalFormation()
    {
        Vector2[] positions = {
            new Vector2(3.0f, 0),
            new Vector2(1.5f, 1.5f),
            new Vector2(0f, 3.0f),
            new Vector2(-1.5f, 4.5f)
        };

        foreach (Vector2 offset in positions)
        {
            Vector3 pos = new Vector3(offset.x, spawnY + offset.y, 0);
            Instantiate(enemyPrefabB, pos, Quaternion.identity);
        }
    }

    #endregion
}