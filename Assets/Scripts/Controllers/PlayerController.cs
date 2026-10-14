using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(PlayerModel))]
public class PlayerController : MonoBehaviour
{
    private PlayerModel _playerModel;
    private Rigidbody2D _rigidBody2d;

    void Awake()
    {
        _playerModel = GetComponent<PlayerModel>();
        _rigidBody2d = GetComponent<Rigidbody2D>();
        transform.localScale = Vector3.one * _playerModel.scale;
    }

    public void Move(float h)
    {
        MoveTo(_rigidBody2d.position.x + h * _playerModel.speed * Time.fixedDeltaTime);
    }

    public void MoveTo(float targetX)
    {
        float maxStep = _playerModel.speed * 3f * Time.fixedDeltaTime;
        float x = Mathf.MoveTowards(_rigidBody2d.position.x, targetX, maxStep);
        x = Mathf.Clamp(x, -_playerModel.limitX, _playerModel.limitX);
        _rigidBody2d.MovePosition(new Vector2(x, _rigidBody2d.position.y));
    }
}
