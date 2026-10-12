using UnityEngine;

/// <summary>
/// CONTROLLER da bola: regras de movimento, reflexão, lançamento e perda.
/// Mantém os métodos da aula (PerfectAngleReflect, AngleChange, CalcBallAngleReflect).
/// Avisa quem tiver interesse por EVENTOS (Attached, Launched, Lost), sem conhecer ninguém.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BallModel))]
public class BallController : MonoBehaviour
{
    public event System.Action Attached;
    public event System.Action Launched;
    public event System.Action Lost;

    private BallModel   _ballModel;
    private Rigidbody2D _rigidBody2d;
    private bool _isAttached;
    private bool _isLaunched;

    public bool IsLaunched => _isLaunched;

    void Awake()
    {
        _ballModel   = GetComponent<BallModel>();
        _rigidBody2d = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!_isLaunched) return;

        // Mantém a velocidade constante e a direção sincronizada com a física
        Vector2 v = _rigidBody2d.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) return;

        _ballModel.direction = EnforceMinVertical(v.normalized);
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }

    // ------------------------------------------------------------ ciclo da bola
    /// <summary>Gruda a bola no player (início da fase ou depois de perder uma vida).</summary>
    public void AttachTo(Transform player)
    {
        _isLaunched = false;
        _isAttached = true;

        _rigidBody2d.linearVelocity = Vector2.zero;
        _rigidBody2d.simulated = false;              // desliga a física enquanto está no player

        transform.SetParent(player, false);
        transform.localPosition = new Vector3(0f, _ballModel.attachOffsetY, 0f);
        transform.localRotation = Quaternion.identity;

        Attached?.Invoke();
    }

    /// <summary>Solta a bola para cima, com uma pequena variação de ângulo. Retorna false se não pôde lançar.</summary>
    public bool Launch()
    {
        if (!_isAttached || _isLaunched) return false;

        transform.SetParent(null, true);
        _rigidBody2d.simulated = true;
        _isAttached = false;
        _isLaunched = true;

        float angleRad = Random.Range(70f, 110f) * Mathf.Deg2Rad;
        AngleChange(new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)));

        Launched?.Invoke();
        return true;
    }

    /// <summary>Para a bola (fim de fase, vida perdida ou game over).</summary>
    public void Stop()
    {
        _isLaunched = false;
        _isAttached = false;
        _rigidBody2d.linearVelocity = Vector2.zero;
        _rigidBody2d.simulated = false;
    }

    /// <summary>Chamado quando a bola cai na zona de perda.</summary>
    public void NotifyLost()
    {
        if (!_isLaunched) return;
        Stop();
        Lost?.Invoke();
    }

    // ------------------------------------------------------ reflexão (da aula)
    /// <summary>Reflexão perfeita (paredes e bricks): espelha a direção pela normal do contato.</summary>
    public void PerfectAngleReflect(Collision2D collision)
    {
        Vector2 normal = collision.GetContact(0).normal;

        // Se a bola já está se afastando (2º contato no mesmo quadro), não reflete de novo
        if (Vector2.Dot(_ballModel.direction, normal) >= 0f) return;

        AngleChange(Vector2.Reflect(_ballModel.direction, normal));
    }

    public void AngleChange(Vector2 dir)
    {
        _ballModel.direction = EnforceMinVertical(dir.normalized);
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }

    /// <summary>
    /// Reflexão no player: o ângulo depende do ponto onde a bola bate (mesma lógica da aula).
    /// Borda direita = 25 graus, centro = 90 graus (vertical), borda esquerda = 155 graus.
    /// A largura do player agora vem do collider (antes era fixa: 120 px / 100).
    /// </summary>
    public void CalcBallAngleReflect(Collision2D playerCol)
    {
        // Metade da largura do player, na escala da Unity (antes: 120 / 2 / 100 = 0.6)
        float halfWidth = playerCol.collider.bounds.extents.x;

        // 0 na borda direita ... 1 na borda esquerda (equivale ao "fator 1.5" da aula)
        float t = Mathf.Clamp01((playerCol.transform.position.x - transform.position.x + halfWidth) / (2f * halfWidth));

        float angleDeg = Mathf.Clamp(t * 180f, 25f, 155f);
        float angleRad = angleDeg * Mathf.Deg2Rad;

        AngleChange(new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)));
    }

    /// <summary>Critério de pontuação: bater no player custa pontos.</summary>
    public void OnPlayerContact()
    {
        GameSession.Instance.AddScore(ScoreRules.PlayerHit);
    }

    // ------------------------------------------------------------------ apoio
    private Vector2 EnforceMinVertical(Vector2 dir)
    {
        float min = _ballModel.minVerticalComponent;
        if (Mathf.Abs(dir.y) >= min) return dir;

        float sy = dir.y >= 0f ? 1f : -1f;
        float sx = dir.x >= 0f ? 1f : -1f;
        return new Vector2(sx * Mathf.Sqrt(1f - min * min), sy * min);
    }
}
