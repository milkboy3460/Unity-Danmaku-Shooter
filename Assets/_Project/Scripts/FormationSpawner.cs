using System.Collections;
using UnityEngine;

// 一定間隔でザコ敵の編隊（ウェーブ）を生成するクラス。
// Update関数でタイマーを回すよりコルーチンの方がスッキリ書けるので、Startからループを回している。
public class FormationSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefabA; // まっすぐ降りてくるやつ
    [SerializeField] private GameObject enemyPrefabB; // 斜めに動くやつ
    [SerializeField] private GameObject enemyPrefabC; // サインカーブでウネウネ動くやつ

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 4.0f; 
    [SerializeField] private float spawnY = 9.0f;        // 画面外（上）の出現位置ベース

    void Start()
    {
        // ゲーム開始と同時にスポーンのループ処理をスタート
        StartCoroutine(SpawnRoutine());
    }

    // ランダムなパターンの編隊を出し続けるメインループ
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

            // ここで指定秒数待機してから次のループへ行く。
            // （Update内でTime.deltaTimeを足し算して管理するより直感的でバグりにくい）
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    #region Formation Patterns

    // パターンA：横一列に並んで同時に降りてくる
    private void SpawnLineFormation()
    {
        float[] xPositions = { -2.5f, 0f, 2.5f }; 
        
        foreach (float x in xPositions)
        {
            Vector3 pos = new Vector3(x, spawnY, 0);
            Instantiate(enemyPrefabA, pos, Quaternion.identity);
        }
    }

    // パターンB：シューティングのお約束、V字編隊
    // 最初からVの字に並べてY座標（高さ）ごとズラしておくことで、
    // プログラムで個別に待機時間を設定しなくても「時間差で画面に入ってくる」ようにしている。
    private void SpawnVFormation()
    {
        Vector2[] positions = {
            new Vector2(0, 0),         // 先頭（中央）一番下なので最初に画面に入る
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

    // パターンC：斜め一列の編隊
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