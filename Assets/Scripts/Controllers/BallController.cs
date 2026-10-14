using UnityEngine;

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

        Vector2 v = _rigidBody2d.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) return;

        _ballModel.direction = EnforceMinVertical(v.normalized);
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }


    public void AttachTo(Transform player)
    {
        _isLaunched = false;
        _isAttached = true;

        _rigidBody2d.linearVelocity = Vector2.zero;
        _rigidBody2d.simulated = false;              

        transform.SetParent(player, false);
        transform.localPosition = new Vector3(0f, _ballModel.attachOffsetY, 0f);
        transform.localRotation = Quaternion.identity;

        Attached?.Invoke();
    }

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

    public void Stop()
    {
        _isLaunched = false;
        _isAttached = false;
        _rigidBody2d.linearVelocity = Vector2.zero;
        _rigidBody2d.simulated = false;
    }

    public void NotifyLost()
    {
        if (!_isLaunched) return;
        Stop();
        Lost?.Invoke();
    }

    
    public void PerfectAngleReflect(Collision2D collision)
    {
        Vector2 normal = collision.GetContact(0).normal;

        if (Vector2.Dot(_ballModel.direction, normal) >= 0f) return;

        AngleChange(Vector2.Reflect(_ballModel.direction, normal));
    }

    public void AngleChange(Vector2 dir)
    {
        _ballModel.direction = EnforceMinVertical(dir.normalized);
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }

    public void CalcBallAngleReflect(Collision2D playerCol)
    {
        float halfWidth = playerCol.collider.bounds.extents.x;

        float t = Mathf.Clamp01((playerCol.transform.position.x - transform.position.x + halfWidth) / (2f * halfWidth));

        float angleDeg = Mathf.Clamp(t * 180f, 25f, 155f);
        float angleRad = angleDeg * Mathf.Deg2Rad;

        AngleChange(new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)));
    }

    public void OnPlayerContact()
    {
        GameSession.Instance.AddScore(ScoreRules.PlayerHit);
    }

    private Vector2 EnforceMinVertical(Vector2 dir)
    {
        float min = _ballModel.minVerticalComponent;
        if (Mathf.Abs(dir.y) >= min) return dir;

        float sy = dir.y >= 0f ? 1f : -1f;
        float sx = dir.x >= 0f ? 1f : -1f;
        return new Vector2(sx * Mathf.Sqrt(1f - min * min), sy * min);
    }
}
