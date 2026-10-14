using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelState { Ready, Playing, Completed, GameOver }


[RequireComponent(typeof(LevelModel))]
public class LevelController : MonoBehaviour
{
    public event System.Action<LevelState> StateChanged;
    public event System.Action<int> LifeLost;
    public event System.Action LevelCompleted;
    public event System.Action GameOverReached;

    [SerializeField] private BallController   _ball;
    [SerializeField] private PlayerController _player;
    [SerializeField] private float _respawnDelay = 0.9f;
    [SerializeField] private float _endDelay     = 2.4f;

    private LevelModel  _levelModel;
    private PlayerModel _playerModel;
    private GameSession _session;
    private int _bricksRemaining;

    public LevelState State { get; private set; }

    void Awake()
    {
        _levelModel  = GetComponent<LevelModel>();
        _playerModel = _player.GetComponent<PlayerModel>();
        _session     = GameSession.Instance;
    }

    void OnEnable()
    {
        BrickController.BrickDestroyed += OnBrickDestroyed;
        _ball.Launched += OnBallLaunched;
        _ball.Lost     += OnBallLost;
        _session.LivesChanged += OnLivesChanged;
    }

    void OnDisable()
    {
        BrickController.BrickDestroyed -= OnBrickDestroyed;
        if (_ball != null)
        {
            _ball.Launched -= OnBallLaunched;
            _ball.Lost     -= OnBallLost;
        }
        _session.LivesChanged -= OnLivesChanged;
    }

    void Start()
    {
        _session.EnsureStarted(_playerModel.life);
        _session.SetLevel(_levelModel.levelNumber);
        _playerModel.life = _session.Lives;

        _bricksRemaining = FindObjectsByType<BrickController>(FindObjectsSortMode.None).Length;

        _ball.AttachTo(_player.transform);
        SetState(LevelState.Ready);
    }

    private void OnBallLaunched()
    {
        SetState(LevelState.Playing);
    }

    private void OnLivesChanged(int lives)
    {
        _playerModel.life = lives;
    }

    private void OnBrickDestroyed(BrickController brick)
    {
        _bricksRemaining--;
        if (_bricksRemaining <= 0 && State == LevelState.Playing)
            StartCoroutine(CompleteRoutine());
    }

    private void OnBallLost()
    {
        if (State != LevelState.Playing) return;

        bool hasLives = _session.LoseLife();
        LifeLost?.Invoke(_session.Lives);

        if (hasLives) StartCoroutine(RespawnRoutine());
        else          StartCoroutine(GameOverRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_respawnDelay);
        _ball.AttachTo(_player.transform);
        SetState(LevelState.Ready);
    }

    private IEnumerator CompleteRoutine()
    {
        SetState(LevelState.Completed);
        _ball.Stop();

        _session.AddScore(ScoreRules.LevelCompleted);
        if (_levelModel.IsLastLevel)
        {
            _session.AddScore(ScoreRules.LifeBonus * _session.Lives);
            _session.FinishGame();
        }

        LevelCompleted?.Invoke();
        yield return new WaitForSeconds(_endDelay);
        SceneManager.LoadScene(_levelModel.nextScene);
    }

    private IEnumerator GameOverRoutine()
    {
        SetState(LevelState.GameOver);
        _session.FinishGame();
        GameOverReached?.Invoke();
        yield return new WaitForSeconds(_endDelay);
        SceneManager.LoadScene(SceneNames.GameOver);
    }

    private void SetState(LevelState state)
    {
        State = state;
        StateChanged?.Invoke(state);
    }
}