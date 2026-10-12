using UnityEngine;

/// <summary>
/// MODEL do player (raquete): dados de movimento e vidas.
/// </summary>
public class PlayerModel : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Velocidade em unidades por segundo")]
    [SerializeField] private float _speed = 9f;
    [Tooltip("Limite horizontal (o player anda de -limite até +limite)")]
    [SerializeField] private float _limitX = 2.4f;

    [Header("Vidas")]
    [Tooltip("Vidas atuais. O valor oficial fica no GameSession (sobrevive às fases); este campo é um espelho. " +
             "Também é a quantidade inicial quando uma fase é aberta direto no editor.")]
    [SerializeField] private int _life = 3;

    [Header("Aparência")]
    [SerializeField] private float _scale = 1f;

    public float speed  { get => _speed;  set => _speed = value; }
    public float limitX { get => _limitX; set => _limitX = value; }
    public int   life   { get => _life;   set => _life = value; }
    public float scale  { get => _scale;  set => _scale = value; }
}
