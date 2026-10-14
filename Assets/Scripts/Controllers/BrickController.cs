using UnityEngine;


public class BrickController : MonoBehaviour
{
    public static event System.Action<BrickController> BrickDestroyed;

    public event System.Action<float> Damaged;

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
