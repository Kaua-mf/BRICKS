using System;
using UnityEngine;

/// <summary>
/// SINGLETON com os dados da partida: pontuação, nome do jogador, vidas e fase atual.
///
/// Por ser uma classe estática de instância única (e não um objeto de cena), ela SOBREVIVE à troca
/// de cenas: a pontuação e as vidas continuam valendo de uma fase para outra.
///
/// Camadas: os Controllers ESCREVEM aqui (AddScore, LoseLife...) e as Views apenas ESCUTAM os eventos
/// (ScoreChanged, LivesChanged...). Assim o Model não conhece nenhuma View.
/// </summary>
public sealed class GameSession
{
    public const int DefaultLives = 3;

    private const string HighScoreKey  = "bricks.highscore";
    private const string PlayerNameKey = "bricks.playername";

    // ---------------------------------------------------------------- Singleton
    private static GameSession _instance;

    public static GameSession Instance
    {
        get
        {
            if (_instance == null) _instance = new GameSession();
            return _instance;
        }
    }

    // Garante estado limpo ao apertar Play (caso o "Enter Play Mode" sem reload de domínio esteja ligado)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { _instance = null; }

    private GameSession()
    {
        HighScore  = PlayerPrefs.GetInt(HighScoreKey, 0);
        PlayerName = PlayerPrefs.GetString(PlayerNameKey, "Jogador");
        _startLives = DefaultLives;
    }

    // ------------------------------------------------------------------ Eventos
    public event Action<int> ScoreChanged;
    public event Action<int> LivesChanged;
    public event Action<int> LevelChanged;

    // ------------------------------------------------------------------- Dados
    private int _startLives;

    public string PlayerName   { get; private set; }
    public int    Score        { get; private set; }
    public int    Lives        { get; private set; }
    public int    CurrentLevel { get; private set; }
    public int    HighScore    { get; private set; }
    public bool   IsStarted    { get; private set; }
    public bool   IsNewRecord  { get; private set; }

    // ----------------------------------------------------------------- Operações
    public void SetPlayerName(string playerName)
    {
        PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Jogador" : playerName.Trim();
        PlayerPrefs.SetString(PlayerNameKey, PlayerName);
        PlayerPrefs.Save();
    }

    /// <summary>Inicia uma partida nova (zera pontos, restaura vidas e volta para a fase 1).</summary>
    public void StartNewGame(string playerName, int lives = DefaultLives)
    {
        SetPlayerName(playerName);
        _startLives  = lives;
        Score        = 0;
        Lives        = lives;
        CurrentLevel = 1;
        IsNewRecord  = false;
        IsStarted    = true;

        ScoreChanged?.Invoke(Score);
        LivesChanged?.Invoke(Lives);
        LevelChanged?.Invoke(CurrentLevel);
    }

    /// <summary>Recomeça mantendo o nome do jogador (usado no "Recomeçar" do game over).</summary>
    public void RestartGame()
    {
        StartNewGame(PlayerName, _startLives);
    }

    /// <summary>Se a fase foi aberta direto no editor (sem passar pelo menu), cria a partida.</summary>
    public void EnsureStarted(int lives)
    {
        if (!IsStarted) StartNewGame(PlayerName, lives);
    }

    public void SetLevel(int level)
    {
        CurrentLevel = level;
        LevelChanged?.Invoke(CurrentLevel);
    }

    /// <summary>Soma (ou subtrai, se negativo) pontos. A pontuação nunca fica abaixo de zero.</summary>
    public void AddScore(int points)
    {
        Score = Mathf.Max(0, Score + points);
        ScoreChanged?.Invoke(Score);
    }

    /// <summary>Remove uma vida. Retorna true se o jogador ainda tem vidas.</summary>
    public bool LoseLife()
    {
        Lives = Mathf.Max(0, Lives - 1);
        LivesChanged?.Invoke(Lives);
        return Lives > 0;
    }

    /// <summary>Chamado no fim da partida (vitória ou game over): registra o recorde.</summary>
    public void FinishGame()
    {
        if (Score > HighScore)
        {
            HighScore   = Score;
            IsNewRecord = true;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }
}
