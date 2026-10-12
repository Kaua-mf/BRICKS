using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Efeitos sonoros disponíveis.</summary>
public enum Sfx
{
    BrickHit,       // bola acertou um brick (que continua vivo)
    BrickBreak,     // brick destruído
    Player,         // bola bateu no player
    Wall,           // bola bateu na parede
    Launch,         // bola lançada
    LoseLife,       // perdeu uma vida
    LevelComplete,  // fase concluída
    Click           // clique de botão
}

/// <summary>
/// Gerenciador de áudio (Singleton que sobrevive às cenas).
///  - Música de fundo: troca sozinha conforme a cena (menu, fases, vitória, game over).
///  - Efeitos: AudioManager.Play(Sfx.X) de qualquer lugar.
/// Os arquivos ficam em Assets/Resources/Audio. Tecla M liga/desliga o som.
/// Não precisa colocar nada na cena: ele se cria sozinho ao iniciar o jogo.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string Folder       = "Audio/";
    private const float  MusicVolume  = 0.45f;
    private const float  SfxVolume    = 0.90f;

    private AudioSource _music;
    private AudioSource _sfx;
    private bool _muted;
    private readonly Dictionary<Sfx, AudioClip> _clips = new Dictionary<Sfx, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        if (Instance != null) return;
        new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _music = gameObject.AddComponent<AudioSource>();
        _music.playOnAwake = false;
        _music.loop        = true;
        _music.volume      = MusicVolume;

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;
        _sfx.volume      = SfxVolume;

        Load(Sfx.BrickHit,      "sfx_brick_hit");
        Load(Sfx.BrickBreak,    "sfx_brick_break");
        Load(Sfx.Player,        "sfx_player");
        Load(Sfx.Wall,          "sfx_wall");
        Load(Sfx.Launch,        "sfx_launch");
        Load(Sfx.LoseLife,      "sfx_lose_life");
        Load(Sfx.LevelComplete, "sfx_level_complete");
        Load(Sfx.Click,         "sfx_click");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void Update()
    {
        if (GameInput.MutePressed() && !IsTyping())
        {
            _muted = !_muted;
            _music.mute = _muted;
            _sfx.mute   = _muted;
        }
    }

    /// <summary>True se o jogador está digitando num campo de texto (ex.: nome no menu): a tecla M não deve mutar.</summary>
    private static bool IsTyping()
    {
        if (EventSystem.current == null) return false;
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        return selected != null && selected.GetComponent<TMP_InputField>() != null;
    }

    private void Load(Sfx id, string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>(Folder + clipName);
        if (clip == null) Debug.LogWarning("AudioManager: clipe não encontrado em Resources/" + Folder + clipName);
        _clips[id] = clip;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case SceneNames.Menu:     PlayMusic("bgm_menu",     true);  break;
            case SceneNames.Victory:  PlayMusic("bgm_victory",  true);  break;
            case SceneNames.GameOver: PlayMusic("bgm_gameover", false); break;
            default:                  PlayMusic("bgm_game",     true);  break; // Fase1, Fase2, Fase3...
        }
    }

    private void PlayMusic(string clipName, bool loop)
    {
        AudioClip clip = Resources.Load<AudioClip>(Folder + clipName);
        if (clip == null) return;

        // Entre uma fase e outra a música não reinicia
        if (_music.clip == clip && _music.isPlaying && loop) return;

        _music.clip = clip;
        _music.loop = loop;
        _music.Play();
    }

    /// <summary>Toca um efeito sonoro (pitch opcional para variar o som).</summary>
    public static void Play(Sfx sfx, float pitch = 1f)
    {
        if (Instance == null) return;
        AudioClip clip;
        if (!Instance._clips.TryGetValue(sfx, out clip) || clip == null) return;
        Instance._sfx.pitch = pitch;
        Instance._sfx.PlayOneShot(clip);
    }
}
