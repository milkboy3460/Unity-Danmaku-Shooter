using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;
using System.Collections.Generic;

#region Data Transfer Objects
// サーバーとやり取りするためのデータ構造（DTO）。
// JsonUtilityで自動変換できるよう [System.Serializable] を付けている。
[System.Serializable]
public class ScoreData { public string playerName; public int score; }

[System.Serializable]
public class PlayerNameData { public string name; }

[System.Serializable]
public class PlayerStatsData 
{ 
    public string name; 
    public int playCount; 
    public int highScore; 
    public List<int> history; 
}
#endregion

// JsonUtilityで配列を直接扱えない（JSONのルートが配列だとパースエラーになる）という
// Unity特有の罠を回避するためのヘルパークラス。
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T> { public T[] array; }
}

// サーバー通信のマネージャー。
// 自作のJava/Spring Bootバックエンドと連携し、ランキング取得やスコア送信を行う。
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

    // プレイヤー登録（POST）
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
            
            // 【重要】
            // Content-Typeを正しく送らないとサーバー側でJSONとして認識されず400エラーになるため明示的に設定。
            request.uploadHandler.contentType = "application/json"; 
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (loadingPopup != null) loadingPopup.SetActive(false);

            if (request.responseCode == 409) // 名前が重複している場合は登録不可
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

    // スコア投稿（POST）
    public void PostFinalScore(int finalScore)
    {
        string playerName = PlayerNameManager.PlayerName;
        if (string.IsNullOrEmpty(playerName)) playerName = "Guest";
        StartCoroutine(SendScoreCoroutine(playerName, finalScore));
    }

    private IEnumerator SendScoreCoroutine(string playerName, int score)
    {
        ScoreData data = new ScoreData { playerName = playerName, score = score };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/score", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.uploadHandler.contentType = "application/json";
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

    // ランキング取得（GET）
    public void FetchRanking()
    {
        StartCoroutine(GetRankingCoroutine());
    }

    private IEnumerator GetRankingCoroutine()
    {
        if (loadingPopup != null) loadingPopup.SetActive(true);
        ClearRankingTexts();

        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/ranking"))
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

    // 統計データ取得（GET）
    public void FetchPlayerStats(string playerName)
    {
        StartCoroutine(GetPlayerStatsCoroutine(playerName));
    }

    private IEnumerator GetPlayerStatsCoroutine(string playerName)
    {
        if (statsLoadingPopup != null) statsLoadingPopup.SetActive(true);

        if (string.IsNullOrEmpty(playerName)) playerName = "Guest";
        
        // 【重要】名前に記号が含まれる場合を想定してURLエンコードを行う
        string encodedName = System.Uri.EscapeDataString(playerName);

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
                rankingTexts[i].text = $"{scores[i].playerName} - {scores[i].score}";
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
            for (int i = 0; i < statsHistoryTexts.Length향; i++)
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