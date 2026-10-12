using UnityEngine;

/// <summary>
/// VIEW da bola: lê a entrada (lançar), detecta as colisões da Unity e delega as regras ao Controller.
/// Também cuida do visual (rastro) e do som de colisão com parede/lançamento.
/// Não altera pontuação nem vidas: isso é papel dos Controllers.
/// </summary>
[RequireComponent(typeof(BallController), typeof(BallModel))]
public class BallView : MonoBehaviour
{
    private BallController _ballController;
    private BallModel      _ballModel;
    private TrailRenderer  _trail;

    void Awake()
    {
        _ballController = GetComponent<BallController>();
        _ballModel      = GetComponent<BallModel>();
        CreateTrail();
    }

    void OnEnable()
    {
        _ballController.Attached += OnAttached;
        _ballController.Launched += OnLaunched;
        _ballController.Lost     += OnLost;
    }

    void OnDisable()
    {
        _ballController.Attached -= OnAttached;
        _ballController.Launched -= OnLaunched;
        _ballController.Lost     -= OnLost;
    }

    void Update()
    {
        if (GameInput.LaunchPressed() && _ballController.Launch())
            AudioManager.Play(Sfx.Launch);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        string otherTag = collision.gameObject.tag;

        if (otherTag == "Enemy")
        {
            // Brick: causa dano (a pontuação e o som são tratados pelo BrickController/BrickView)
            BrickView brickView = collision.gameObject.GetComponent<BrickView>();
            if (brickView != null) brickView.PerformTakeDamage(_ballModel.power);
            _ballController.PerfectAngleReflect(collision);
        }
        else if (otherTag == "Player")
        {
            // Player: ângulo calculado pelo ponto de contato + penalidade de pontos
            _ballController.CalcBallAngleReflect(collision);
            _ballController.OnPlayerContact();
        }
        else
        {
            // Paredes
            _ballController.PerfectAngleReflect(collision);
            AudioManager.Play(Sfx.Wall, Random.Range(0.95f, 1.08f));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeadZone"))
            _ballController.NotifyLost();
    }

    // ---------------------------------------------------------------- visual
    private void CreateTrail()
    {
        _trail = gameObject.AddComponent<TrailRenderer>();
        _trail.time           = 0.18f;
        _trail.startWidth     = 0.22f;
        _trail.endWidth       = 0f;
        _trail.numCapVertices = 4;
        _trail.sortingOrder   = 9;
        _trail.startColor     = new Color(0.85f, 0.92f, 1f, 0.55f);
        _trail.endColor       = new Color(0.60f, 0.40f, 1f, 0f);
        _trail.emitting       = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) _trail.material = new Material(shader);
    }

    private void OnAttached()
    {
        _trail.emitting = false;
        _trail.Clear();
    }

    private void OnLaunched()
    {
        _trail.Clear();
        _trail.emitting = true;
    }

    private void OnLost()
    {
        _trail.emitting = false;
    }
}
