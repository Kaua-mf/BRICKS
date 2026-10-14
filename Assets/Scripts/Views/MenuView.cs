using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] private GameFlowController _flow;
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Button   _playButton;
    [SerializeField] private Button   _quitButton;
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private RectTransform _logo;

    void Start()
    {
        GameSession s = GameSession.Instance;
        _nameInput.text = s.PlayerName == "Jogador" ? "" : s.PlayerName;
        _highScoreText.text = "RECORDE: " + s.HighScore;

        _playButton.onClick.AddListener(OnPlay);
        _quitButton.onClick.AddListener(OnQuit);
        _nameInput.onSubmit.AddListener(OnSubmit);
    }

    void OnDestroy()
    {
        _playButton.onClick.RemoveListener(OnPlay);
        _quitButton.onClick.RemoveListener(OnQuit);
        _nameInput.onSubmit.RemoveListener(OnSubmit);
    }

    void Update()
    {
        // Logo "respirando"
        _logo.localScale = Vector3.one * (1f + 0.025f * Mathf.Sin(Time.time * 2f));
    }

    private void OnSubmit(string text) { OnPlay(); }

    private void OnPlay()
    {
        AudioManager.Play(Sfx.Click);
        _flow.StartNewGame(_nameInput.text);
    }

    private void OnQuit()
    {
        AudioManager.Play(Sfx.Click);
        _flow.Quit();
    }
}
