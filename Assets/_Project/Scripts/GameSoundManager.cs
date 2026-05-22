using UnityEngine;

// ゲーム全体の音（BGM・SE）を管理するシングルトン。
// 初心者がよくやる「SEが鳴るたびにBGMが一瞬途切れるバグ」を防ぐため、
// BGM用のスピーカー(AudioSource)とSE用のスピーカーを完全に分けて独立稼働させている。
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

    // SE用のスピーカー（Inspectorでアタッチ済みのものを使う）
    private AudioSource seAudioSource;  
    
    // BGM用のスピーカー（インスペクタの付け忘れバグを防ぐため、スクリプトから自動生成する）
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

        // 【バグ対策】
        // BGMとSEを同時に鳴らすため、BGM専用のAudioSourceを起動時に裏でこっそり追加する。
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

    // ゲームオーバー時のBGM切り替え。
    // 古いBGMと新しいBGMが被ってカオスになるのを防ぐため、必ずStopを挟んでから流す。
    public void PlayGameOverBGM()
    {
        if (bgmAudioSource != null && gameOverBgm != null)
        {
            bgmAudioSource.Stop(); 
            bgmAudioSource.clip = gameOverBgm;
            bgmAudioSource.Play(); 
        }
    }

    // やられた瞬間のヒットストップ演出などで、音をピタッと止めたい時に使う
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
    // 【重要】
    // 弾幕シューティングは尋常じゃない回数のSEが鳴るため、通常の Play() は使わない。
    // 必ず PlayOneShot() を使い、既に鳴っている音を止めずに上から重ねて再生させる。
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