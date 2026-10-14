using UnityEngine;


public class LevelModel : MonoBehaviour
{
    [SerializeField] private int    _levelNumber = 1;
    [SerializeField] private string _levelTitle  = "Fase 1";
    [Tooltip("Nome da cena que será carregada ao concluir esta fase")]
    [SerializeField] private string _nextScene   = SceneNames.Fase2;

    public int    levelNumber { get => _levelNumber; set => _levelNumber = value; }
    public string levelTitle  { get => _levelTitle;  set => _levelTitle  = value; }
    public string nextScene   { get => _nextScene;   set => _nextScene   = value; }

    public bool IsLastLevel => _nextScene == SceneNames.Victory;
}
