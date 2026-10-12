using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VIEW do HUD (demonstrativo em tela): pontuação, vidas (corações), fase, nome do jogador e mensagens.
///
/// INDEPENDÊNCIA DE CAMADAS: esta View só LÊ e ESCUTA. Ela assina os eventos do Singleton GameSession
/// (ScoreChanged, LivesChanged, LevelChanged) e do LevelController (StateChanged) e nunca altera dados.
/// </summary>
public class HudView : MonoBehaviour
{
    [Header("Fonte dos dados")]
    [SerializeField] private LevelController _level;

    [Header("Textos")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _levelText;

    [Header("Vidas")]
    [SerializeField] private Image[] _hearts;
    [SerializeField] private Sprite  _heartFull;
    [SerializeField] private Sprite  _heartEmpty;

    [Header("Mensagem central")]
    [SerializeField] private GameObject _messageRoot;
    [SerializeField] private TMP_Text   _messageText;

    private int _lastLives = -1;
    private int _lastScore = 0;
    private Coroutine _scoreRoutine;
    private Coroutine _heartRoutine;

    void OnEnable()
    {
        GameSession s = GameSession.Instance;
        s.ScoreChanged += UpdateScore;
        s.LivesChanged += UpdateLives;
        s.LevelChanged += UpdateLevel;
        if (_level != null) _level.StateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        GameSession s = GameSession.Instance;
        s.ScoreChanged -= UpdateScore;
        s.LivesChanged -= UpdateLives;
        s.LevelChanged -= UpdateLevel;
        if (_level != null) _level.StateChanged -= OnStateChanged;
    }

    void Start()
    {
        GameSession s = GameSession.Instance;
        _lastScore = s.Score;
        _nameText.text  = s.PlayerName;
        _scoreText.text = "PONTOS  " + s.Score;
        UpdateLives(s.Lives);
        UpdateLevel(s.CurrentLevel);
    }

    // ------------------------------------------------------------------ reações
    private void UpdateScore(int score)
    {
        int delta = score - _lastScore;
        _lastScore = score;
        _scoreText.text = "PONTOS  " + score;

        if (delta != 0)
        {
            if (_scoreRoutine != null) StopCoroutine(_scoreRoutine);
            _scoreRoutine = StartCoroutine(Flash(_scoreText, delta < 0 ? new Color(1f, 0.35f, 0.35f) : new Color(1f, 0.92f, 0.45f)));
        }
    }

    private void UpdateLives(int lives)
    {
        bool lost = _lastLives >= 0 && lives < _lastLives;
        _lastLives = lives;

        for (int i = 0; i < _hearts.Length; i++)
            _hearts[i].sprite = i < lives ? _heartFull : _heartEmpty;

        if (lost && lives < _hearts.Length)
        {
            if (_heartRoutine != null) StopCoroutine(_heartRoutine);
            _heartRoutine = StartCoroutine(Pulse(_hearts[lives].rectTransform, 1.6f));
        }
    }

    private void UpdateLevel(int level)
    {
        _levelText.text = "FASE " + level;
        _nameText.text  = GameSession.Instance.PlayerName;
    }

    private void OnStateChanged(LevelState state)
    {
        switch (state)
        {
            case LevelState.Ready:     ShowMessage("Clique ou ESPAÇO para lançar"); break;
            case LevelState.Playing:   HideMessage();                               break;
            case LevelState.Completed: ShowMessage("FASE CONCLUÍDA!");              break;
            case LevelState.GameOver:  ShowMessage("FIM DE JOGO");                  break;
        }
    }

    private void ShowMessage(string text)
    {
        _messageText.text = text;
        _messageRoot.SetActive(true);
    }

    private void HideMessage()
    {
        _messageRoot.SetActive(false);
    }

    // --------------------------------------------------------------- animações
    private IEnumerator Flash(TMP_Text text, Color flash)
    {
        const float duration = 0.35f;
        RectTransform rt = text.rectTransform;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = 1f - t / duration;
            text.color = Color.Lerp(Color.white, flash, k);
            rt.localScale = Vector3.one * (1f + 0.10f * k);
            yield return null;
        }
        text.color = Color.white;
        rt.localScale = Vector3.one;
    }

    private IEnumerator Pulse(RectTransform rt, float peak)
    {
        const float duration = 0.4f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            rt.localScale = Vector3.one * (1f + (peak - 1f) * k);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}
