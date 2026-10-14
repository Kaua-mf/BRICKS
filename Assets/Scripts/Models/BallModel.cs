using UnityEngine;


public class BallModel : MonoBehaviour
{
    [Header("Física")]
    [Tooltip("Dano causado em cada brick atingido")]
    [SerializeField] private float _power = 1f;
    [Tooltip("Velocidade em unidades por segundo")]
    [SerializeField, Range(2f, 14f)] private float _speed = 6f;
    [SerializeField] private Vector2 _direction = Vector2.up;

    [Header("Controle")]
    [Tooltip("Altura da bola em relação ao centro do player enquanto aguarda o lançamento")]
    [SerializeField] private float _attachOffsetY = 0.30f;
    [Tooltip("Componente vertical mínima da direção (evita a bola quicar quase na horizontal)")]
    [SerializeField, Range(0.1f, 0.5f)] private float _minVerticalComponent = 0.25f;

    public float   power                { get => _power;                set => _power = value; }
    public float   speed                { get => _speed;                set => _speed = value; }
    public Vector2 direction            { get => _direction;            set => _direction = value; }
    public float   attachOffsetY        { get => _attachOffsetY;        set => _attachOffsetY = value; }
    public float   minVerticalComponent { get => _minVerticalComponent; set => _minVerticalComponent = value; }
}
