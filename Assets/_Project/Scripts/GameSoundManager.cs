using UnityEngine;

/// <summary>
/// ゲーム全体のBGMとSEを統括するシングルトンマネージャー。
/// BGM用とSE用でAudioSourceを分離し、効果音再生時のBGM途切れを防ぐ。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GameSoundManager : MonoBehaviour
{
    public static GameSoundManager Instance { get; private set; }

    [Header("BGM Tracks")]
    [SerializeField] private AudioClip mainBgm;       
    [SerializeField] private AudioClip gameOverBgm;   

    [Header("Common SE (Player/Enemy)")]
    [SerializeField] private AudioClip shootSound;      
    [SerializeField] private AudioClip explosionSound;  
    [SerializeField] private AudioClip damageSound;     

    [Header("Mid Boss SE")]
    [SerializeField] private AudioClip midBossShootSound;      
    [SerializeField] private AudioClip midBossDamageSound;     
    [SerializeField] private AudioClip midBossExplosionSound;  

    [Header("True Boss SE")]
    [SerializeField] private AudioClip trueBossShootSound;      
    [SerializeField] private AudioClip trueBossDamageSound;     
    [SerializeField] private AudioClip trueBossExplosionSound;  

    [Header("Item SE")]
    [SerializeField] private AudioClip powerUpSound; 
    [SerializeField] private AudioClip healSound;    

    // SE用のスピーカー（アタッチ済みのコンポーネントを使用）
    private AudioSource seAudioSource;  
    
    // BGM用のスピーカー（動的に生成して使用）
    private AudioSource bgmAudioSource; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        seAudioSource = GetComponent<AudioSource>();

        // BGMとSEの同時再生（多重再生）を可能にするため、BGM専用のAudioSourceを動的に追加
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true;  
        bgmAudioSource.volume = 0.5f; 
    }

    void Start()
    {
        PlayMainBGM();
    }

    #region BGM Control

    public void PlayMainBGM()
    {
        if (bgmAudioSource != null && mainBgm != null)
        {
            bgmAudioSource.clip = mainBgm;
            bgmAudioSource.Play(); 
        }
    }

    /// <summary>
    /// ゲームオーバー時のBGM再生。
    /// 既存のBGMを明示的に停止してから新しいクリップを再生し、音の重なりを防ぐ。
    /// </summary>
    public void PlayGameOverBGM()
    {
        if (bgmAudioSource != null && gameOverBgm != null)
        {
            bgmAudioSource.Stop(); 
            bgmAudioSource.clip = gameOverBgm;
            bgmAudioSource.Play(); 
        }
    }

    /// <summary>
    /// BGMの停止（ゲームオーバー演出のスローモーション時などに使用）
    /// </summary>
    public void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    #endregion

    #region SE Playback Methods
    // ----------------------------------------------------------------------
    // 注意：SEはすべて PlayOneShot を使用し、既存の音を止めずに重ねて再生する
    // ----------------------------------------------------------------------

    public void PlayShootSound()
    {
        if (shootSound != null && seAudioSource != null) seAudioSource.PlayOneShot(shootSound);
    }

    public void PlayExplosionSound()
    {
        if (explosionSound != null && seAudioSource != null) seAudioSource.PlayOneShot(explosionSound);
    }

    public void PlayDamageSound()
    {
        if (damageSound != null && seAudioSource != null) seAudioSource.PlayOneShot(damageSound);
    }

    public void PlayMidBossShootSound()
    {
        if (midBossShootSound != null && seAudioSource != null) seAudioSource.PlayOneShot(midBossShootSound);
    }

    public void PlayMidBossDamageSound()
    {
        if (midBossDamageSound != null && seAudioSource != null) seAudioSource.PlayOneShot(midBossDamageSound);
    }

    public void PlayMidBossExplosionSound()
    {
        if (midBossExplosionSound != null && seAudioSource != null) seAudioSource.PlayOneShot(midBossExplosionSound);
    }

    public void PlayTrueBossShootSound()
    {
        if (trueBossShootSound != null && seAudioSource != null) seAudioSource.PlayOneShot(trueBossShootSound);
    }

    public void PlayTrueBossDamageSound()
    {
        if (trueBossDamageSound != null && seAudioSource != null) seAudioSource.PlayOneShot(trueBossDamageSound);
    }

    public void PlayTrueBossExplosionSound()
    {
        if (trueBossExplosionSound != null && seAudioSource != null) seAudioSource.PlayOneShot(trueBossExplosionSound);
    }

    public void PlayPowerUpSound()
    {
        if (powerUpSound != null && seAudioSource != null) seAudioSource.PlayOneShot(powerUpSound);
    }

    public void PlayHealSound()
    {
        if (healSound != null && seAudioSource != null) seAudioSource.PlayOneShot(healSound);
    }

    #endregion
}