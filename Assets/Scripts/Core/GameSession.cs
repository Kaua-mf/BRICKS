using System;
using UnityEngine;


public sealed class GameSession
{
    public const int DefaultLives = 3;

    private const string HighScoreKey  = "bricks.highscore";
    private const string PlayerNameKey = "bricks.playername";

    private static GameSession _instance;

    public static GameSession Instance
    {
        get
        {
            if (_instance == null) _instance = new GameSession();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { _instance = null; }

    private GameSession()
    {
        HighScore  = PlayerPrefs.GetInt(HighScoreKey, 0);
        PlayerName = PlayerPrefs.GetString(PlayerNameKey, "Jogador");
        _startLives = DefaultLives;
    }

    public event Action<int> ScoreChanged;
    public event Action<int> LivesChanged;
    public event Action<int> LevelChanged;

    private int _startLives;

    public string PlayerName   { get; private set; }
    public int    Score        { get; private set; }
    public int    Lives        { get; private set; }
    public int    CurrentLevel { get; private set; }
    public int    HighScore    { get; private set; }
    public bool   IsStarted    { get; private set; }
    public bool   IsNewRecord  { get; private set; }

    public void SetPlayerName(string playerName)
    {
        PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Jogador" : playerName.Trim();
        PlayerPrefs.SetString(PlayerNameKey, PlayerName);
        PlayerPrefs.Save();
    }

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

    public void RestartGame()
    {
        StartNewGame(PlayerName, _startLives);
    }

    public void EnsureStarted(int lives)
    {
        if (!IsStarted) StartNewGame(PlayerName, lives);
    }

    public void SetLevel(int level)
    {
        CurrentLevel = level;
        LevelChanged?.Invoke(CurrentLevel);
    }

    public void AddScore(int points)
    {
        Score = Mathf.Max(0, Score + points);
        ScoreChanged?.Invoke(Score);
    }

    public bool LoseLife()
    {
        Lives = Mathf.Max(0, Lives - 1);
        LivesChanged?.Invoke(Lives);
        return Lives > 0;
    }

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
