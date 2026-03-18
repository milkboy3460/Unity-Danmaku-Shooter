using UnityEngine;

/// <summary>
/// スコアに応じた中ボスの出現と、撃破数に基づく真ボスの出現を管理するスポナークラス。
/// 真のボス撃破後は進行状況をリセットし、再び中ボスサイクルに移行する（無限ループ構造）。
/// </summary>
public class BossSpawner : MonoBehaviour
{
    [Header("Mid Boss Settings")]
    [SerializeField] private GameObject midBossPrefab; 
    [SerializeField] private int firstSpawnScore = 1000;   // 初回およびループ再開時の出現に必要な加算スコア
    [SerializeField] private int nextSpawnInterval = 6000; // 2回目以降の出現間隔スコア
    [SerializeField] private Vector3 startPosition = new Vector3(0, 8.0f, 0);

    [Header("True Boss Settings")]
    [SerializeField] private GameObject trueBossPrefab; 
    [SerializeField] private int requiredDefeats = 5;      // 真ボス出現までに必要な中ボスの規定撃破数

    private int nextTargetScore;
    private GameObject currentBoss;
    private GameObject currentTrueBoss; 

    private int defeatedCount = 0;      
    private bool wasBossAlive = false;  
    private bool hasSpawnedTrueBoss = false; 

    void Start()
    {
        nextTargetScore = firstSpawnScore;
    }

    void Update()
    {
        // フェーズ1：真のボス出現中
        if (hasSpawnedTrueBoss)
        {
            // 真のボス撃破（オブジェクト消滅）を検知してサイクルをリセット
            if (currentTrueBoss == null)
            {
                Debug.Log("真のボス撃破！中ボスサイクルを再開します！");
                
                hasSpawnedTrueBoss = false; 
                defeatedCount = 0;          
                wasBossAlive = false;

                // ゲームテンポがダレるのを防ぐため、ループ再開時は初期の短い出現スパン（firstSpawnScore）を適用する
                if (ScoreManager.instance != null)
                {
                    nextTargetScore = ScoreManager.instance.GetCurrentScore() + firstSpawnScore;
                }
            }
            return; 
        }

        // フェーズ2：中ボスの撃破判定（生存状態からnullになった瞬間をエッジとして検知）
        if (wasBossAlive && currentBoss == null)
        {
            wasBossAlive = false;
            defeatedCount++;
            
            // ログ出力をC#のモダンな文字列補間（String Interpolation）に修正
            Debug.Log($"中ボス撃破数: {defeatedCount} / {requiredDefeats}");

            // 規定数に達したら真のボスをスポーンさせ、中ボスサイクルを一時中断する
            if (defeatedCount >= requiredDefeats)
            {
                SpawnTrueBoss();
                return; 
            }
        }

        // フェーズ3：通常の中ボス出現判定（目標スコア到達時）
        if (currentBoss == null && ScoreManager.instance != null && ScoreManager.instance.GetCurrentScore() >= nextTargetScore)
        {
            SpawnMidBoss();
        }
    }

    private void SpawnMidBoss()
    {
        if (midBossPrefab != null)
        {
            currentBoss = Instantiate(midBossPrefab, startPosition, Quaternion.identity);
            wasBossAlive = true; 
            
            // 次回出現のための目標スコアを更新
            nextTargetScore = ScoreManager.instance.GetCurrentScore() + nextSpawnInterval;
        }
    }

    private void SpawnTrueBoss()
    {
        if (trueBossPrefab != null)
        {
            currentTrueBoss = Instantiate(trueBossPrefab, startPosition, Quaternion.identity);
            hasSpawnedTrueBoss = true; 
            Debug.Log("警告：真のボスが出現しました！！");
        }
    }
}