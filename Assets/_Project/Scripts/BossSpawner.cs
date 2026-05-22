using UnityEngine;

// スコアに応じて中ボスを出し、規定数倒したら真ボスを出すスポナー。
// 真ボスを倒した後はまた中ボスから始まる（無限ループ仕様）。
public class BossSpawner : MonoBehaviour
{
    [Header("Mid Boss Settings")]
    [SerializeField] private GameObject midBossPrefab; 
    [SerializeField] private int firstSpawnScore = 1000;   // 最初の1体目、またはループ再開時に必要なスコア
    [SerializeField] private int nextSpawnInterval = 6000; // ボスを倒した後、次のボスが出るまでに稼ぐスコア
    [SerializeField] private Vector3 startPosition = new Vector3(0, 8.0f, 0);

    [Header("True Boss Settings")]
    [SerializeField] private GameObject trueBossPrefab; 
    [SerializeField] private int requiredDefeats = 5;      // 真ボスを出すまでに必要な中ボスの撃破数

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
        // 状態1：真ボスと戦闘中
        if (hasSpawnedTrueBoss)
        {
            // 真ボスを倒した（オブジェクトが消滅した）らサイクルを最初に戻す
            if (currentTrueBoss == null)
            {
                Debug.Log("真のボス撃破！中ボスサイクルを再開します！");
                
                hasSpawnedTrueBoss = false; 
                defeatedCount = 0;          
                wasBossAlive = false;

                // ループ再開時、なかなか次のボスが出ないとテンポがダレるので初回用の短いスパンをセットする
                if (ScoreManager.instance != null)
                {
                    nextTargetScore = ScoreManager.instance.GetCurrentScore() + firstSpawnScore;
                }
            }
            return; 
        }

        // 状態2：中ボスを倒した瞬間の判定（さっきまで生きてて、今nullになったら撃破とみなす）
        if (wasBossAlive && currentBoss == null)
        {
            wasBossAlive = false;
            defeatedCount++;
            
            Debug.Log($"中ボス撃破数: {defeatedCount} / {requiredDefeats}");

            // 規定数倒したら真ボスを出して、中ボスサイクルはお休み
            if (defeatedCount >= requiredDefeats)
            {
                SpawnTrueBoss();
                return; 
            }

            // ★連戦防止のための重要処理★
            // 「ボスが出た時」ではなく「ボスを倒した瞬間」のスコアを基準に次回の目標スコアを設定する。
            // これをやらないと、ボス戦中にザコを倒してスコアを稼ぎすぎた場合、ボス撃破直後に次のボスが即湧きしてしまう。
            if (ScoreManager.instance != null)
            {
                nextTargetScore = ScoreManager.instance.GetCurrentScore() + nextSpawnInterval;
            }
        }

        // 状態3：ボスがいなくて、目標スコアに達していたら中ボスを出す
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
            
            // （※以前はここで次の目標スコアを計算していたが、連戦バグの原因になるため撃破時の処理に移動した）
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