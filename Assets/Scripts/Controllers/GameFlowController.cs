using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CONTROLLER do fluxo entre telas: iniciar jogo, RECOMEÇAR após o game over, voltar ao menu e sair.
/// Usado pelas cenas Menu, Vitoria e GameOver.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    /// <summary>Tela de abertura -> começa a partida com o nome informado.</summary>
    public void StartNewGame(string playerName)
    {
        GameSession.Instance.StartNewGame(playerName);
        SceneManager.LoadScene(SceneNames.Fase1);
    }

    /// <summary>Recomeçar (game over / vitória): zera pontos e vidas e volta para a fase 1.</summary>
    public void RestartGame()
    {
        GameSession.Instance.RestartGame();
        SceneManager.LoadScene(SceneNames.Fase1);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(SceneNames.Menu);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
