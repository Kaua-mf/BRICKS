using UnityEngine;
using UnityEngine.SceneManagement;


public class GameFlowController : MonoBehaviour
{
    public void StartNewGame(string playerName)
    {
        GameSession.Instance.StartNewGame(playerName);
        SceneManager.LoadScene(SceneNames.Fase1);
    }

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
