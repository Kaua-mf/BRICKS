using UnityEngine;

/// <summary>
/// CONTROLLER do brick: aplica o dano, pontua e destrói.
/// Funciona com QUALQUER tipo de brick porque só conhece a classe abstrata BrickModel (polimorfismo).
/// Avisa por eventos (Damaged, Destroying, BrickDestroyed) para a View e o LevelController reagirem.
/// </summary>
public class BrickController : MonoBehaviour
{
    /// <summary>Evento estático: qualquer brick destruído (o LevelController conta por aqui).</summary>
    public static event System.Action<BrickController> BrickDestroyed;

    /// <summary>Brick sofreu dano e continua vivo. Parâmetro: vida restante (0..1).</summary>
    public event System.Action<float> Damaged;

    /// <summary>Brick vai ser destruído agora (a View toca som/partículas).</summary>
    public event System.Action Destroying;

    private BrickModel _brickModel;
    private bool _destroyed;

    void Awake()
    {
        _brickModel = GetComponent<BrickModel>();
    }

    public void TakeDamage(float damage)
    {
        if (_destroyed) return;

        _brickModel.Health -= damage;

        if (_brickModel.Health <= 0f)
        {
            _destroyed = true;
            GameSession.Instance.AddScore(_brickModel.PointsOnDestroy);
            Destroying?.Invoke();
            BrickDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
        else
        {
            GameSession.Instance.AddScore(_brickModel.PointsPerHit);
            Damaged?.Invoke(_brickModel.Health / _brickModel.MaxHealth);
        }
    }
}
