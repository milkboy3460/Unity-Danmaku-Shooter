using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;
using System.Collections.Generic;

#region Data Transfer Objects
[System.Serializable]
public class ScoreData
{
    public string name;
    public int score;
}

[System.Serializable]
public class PlayerNameData
{
    public string name;
}

[System.Serializable]
public class PlayerStatsData
{
    public string name;
    public int playCount;
    public int highScore;
    public List<int> history; 
}
#endregion

/// <summary>
/// Unity標準のJsonUtilityでトップレベルの配列を扱うためのユーティリティ。
/// </summary>
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

/// <summary>
/// 外部サーバーのAPIと通信し、ランキングや戦績データの同期を管理するクラス。
/// </summary>
public class RankingNetworkManager : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string baseUrl = "https://unity-tetris-server.onrender.com/api";

    [Header("Ranking UI References")]
    [SerializeField] private TextMeshProUGUI[] rankingTexts; 
    [SerializeField] private GameObject loadingPopup; 

    [Header("Stats UI References")]
    [SerializeField] private TextMeshProUGUI statsNameText;
    [SerializeField] private TextMeshProUGUI statsPlayCountText;
    [SerializeField] private TextMeshProUGUI statsHighScoreText;
    [SerializeField] private TextMeshProUGUI[] statsHistoryTexts;
    [SerializeField] private GameObject statsLoadingPopup; 

    /// <summary>
    /// プレイヤー名の新規登録。HTTP 409 (Conflict) を重複としてハンドリングする。
    /// </summary>
    public void RegisterPlayer(string playerName, System.Action<bool> onResult)
    {
        StartCoroutine(RegisterPlayerCoroutine(playerName, onResult));
    }

    private IEnumerator RegisterPlayerCoroutine(string playerName, System.Action<bool> onResult)
    {
        if (loadingPopup != null) loadingPopup.SetActive(true);

        PlayerNameData data = new PlayerNameData { name = playerName };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/players/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (loadingPopup != null) loadingPopup.SetActive(false);

            // Javaバックエンド側で定義した重複エラー(409)を判定
            if (request.responseCode == 409)
            {
                onResult?.Invoke(false); 
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                onResult?.Invoke(true); 
            }
            else
            {
                onResult?.Invoke(false); 
            }
        }
    }

    /// <summary>
    /// ゲーム終了時の最終スコアを送信する。
    /// </summary>
    public void PostFinalScore(int finalScore)
    {
        string playerName = PlayerNameManager.PlayerName;
        if (string.IsNullOrEmpty(playerName)) playerName = "???";
        StartCoroutine(SendScoreCoroutine(playerName, finalScore));
    }

    private IEnumerator SendScoreCoroutine(string playerName, int score)
    {
        ScoreData data = new ScoreData { name = playerName, score = score };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/scores", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Score posted successfully: {playerName} - {score}");
            }
            else
            {
                Debug.LogError($"Score post error: {request.error}");
            }
        }
    }

    /// <summary>
    /// サーバーから全ランキングデータを取得し、UIを更新する。
    /// </summary>
    public void FetchRanking()
    {
        StartCoroutine(GetRankingCoroutine());
    }

    private IEnumerator GetRankingCoroutine()
    {
        if (loadingPopup != null) loadingPopup.SetActive(true);
        ClearRankingTexts();

        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/scores"))
        {
            yield return request.SendWebRequest();

            if (loadingPopup != null) loadingPopup.SetActive(false);

            if (request.result != UnityWebRequest.Result.Success)
            {
                ShowErrorText(); 
            }
            else
            {
                UpdateRankingUI(request.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// 指定されたプレイヤーの詳細戦績を取得する。
    /// </summary>
    public void FetchPlayerStats(string playerName)
    {
        StartCoroutine(GetPlayerStatsCoroutine(playerName));
    }

    private IEnumerator GetPlayerStatsCoroutine(string playerName)
    {
        if (statsLoadingPopup != null) statsLoadingPopup.SetActive(true);

        // URLに含める名前を安全にエンコード（スペース等の考慮）
        string encodedName = UnityWebRequest.EscapeURL(playerName);
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/players/{encodedName}/stats"))
        {
            yield return request.SendWebRequest();

            if (statsLoadingPopup != null) statsLoadingPopup.SetActive(false);

            if (request.result == UnityWebRequest.Result.Success)
            {
                PlayerStatsData stats = JsonUtility.FromJson<PlayerStatsData>(request.downloadHandler.text);
                UpdateStatsUI(stats);
            }
            else
            {
                Debug.LogError($"Stats fetch error: {request.error}");
            }
        }
    }

    private void ClearRankingTexts()
    {
        if (rankingTexts == null) return;
        foreach (var text in rankingTexts)
        {
            if (text != null) text.text = "";
        }
    }

    private void ShowErrorText()
    {
        if (rankingTexts != null && rankingTexts.Length > 0 && rankingTexts[0] != null)
        {
            rankingTexts[0].text = "NETWORK ERROR";
        }
    }

    private void UpdateRankingUI(string json)
    {
        if (rankingTexts == null || rankingTexts.Length == 0) return;
        ScoreData[] scores = JsonHelper.FromJson<ScoreData>(json);
        
        for (int i = 0; i < rankingTexts.Length; i++)
        {
            if (i < scores.Length)
            {
                rankingTexts[i].text = $"{scores[i].name} - {scores[i].score}";
            }
            else
            {
                rankingTexts[i].text = "--- - 0";
            }
        }
    }

    private void UpdateStatsUI(PlayerStatsData stats)
    {
        if (statsNameText != null) statsNameText.text = $"NAME: {stats.name}";
        if (statsPlayCountText != null) statsPlayCountText.text = $"PLAY COUNT: {stats.playCount}";
        if (statsHighScoreText != null) statsHighScoreText.text = $"HIGH SCORE: {stats.highScore}";

        if (statsHistoryTexts != null)
        {
            for (int i = 0; i < statsHistoryTexts.Length; i++)
            {
                if (stats.history != null && i < stats.history.Count)
                {
                    statsHistoryTexts[i].text = $"{i + 1}. {stats.history[i]}";
                }
                else
                {
                    statsHistoryTexts[i].text = $"{i + 1}. --- - 0";
                }
            }
        }
    }
}