using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenView : MonoBehaviour
{
    [SerializeField] private GameFlowController _flow;
    [SerializeField] private bool _isVictory;

    [SerializeField] private TMP_Text   _subtitleText;
    [SerializeField] private TMP_Text   _scoreText;
    [SerializeField] private TMP_Text   _highScoreText;
    [SerializeField] private GameObject _recordRoot;
    [SerializeField] private RectTransform _title;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _quitButton;

    void Start()
    {
        GameSession s = GameSession.Instance;

        _subtitleText.text = _isVictory
            ? "Parabéns, " + s.PlayerName + "! Você concluiu todas as fases."
            : "Não foi dessa vez, " + s.PlayerName + ". Tente novamente!";
        _highScoreText.text = "Recorde: " + s.HighScore;
        _recordRoot.SetActive(s.IsNewRecord);

        _restartButton.onClick.AddListener(OnRestart);
        _menuButton.onClick.AddListener(OnMenu);
        _quitButton.onClick.AddListener(OnQuit);

        StartCoroutine(CountUp(s.Score));
    }

    void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(OnRestart);
        _menuButton.onClick.RemoveListener(OnMenu);
        _quitButton.onClick.RemoveListener(OnQuit);
    }

    void Update()
    {
        _title.localScale = Vector3.one * (1f + 0.03f * Mathf.Sin(Time.time * 2.4f));

        if (GameInput.ConfirmPressed())  OnRestart();
        if (GameInput.EscapePressed())   OnMenu();
    }

    private IEnumerator CountUp(int finalScore)
    {
        const float duration = 1.2f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / duration, 3f);
            _scoreText.text = Mathf.RoundToInt(finalScore * k).ToString();
            yield return null;
        }
        _scoreText.text = finalScore.ToString();
    }

    private void OnRestart() { AudioManager.Play(Sfx.Click); _flow.RestartGame(); }
    private void OnMenu()    { AudioManager.Play(Sfx.Click); _flow.GoToMenu(); }
    private void OnQuit()    { AudioManager.Play(Sfx.Click); _flow.Quit(); }
}
