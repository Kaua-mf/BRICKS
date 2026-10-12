using System.Collections;
using UnityEngine;

/// <summary>
/// VIEW do player: lê o teclado/mouse/toque, envia o movimento ao Controller e
/// dá o feedback visual/sonoro quando a bola bate (efeito de som do player).
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerView : MonoBehaviour
{
    private PlayerController _playerController;
    private Camera  _camera;
    private Vector3 _baseScale;
    private float   _h;
    private bool    _useMouse;
    private float   _targetX;
    private Vector2 _lastPointer;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        _camera      = Camera.main;
        _baseScale   = transform.localScale;   // já com a escala aplicada pelo PlayerController
        _lastPointer = GameInput.PointerPosition();
    }

    void Update()
    {
        _h = GameInput.Horizontal();

        Vector2 p = GameInput.PointerPosition();
        bool moved = (p - _lastPointer).sqrMagnitude > 1f;
        _lastPointer = p;

        // Mouse/toque só assume o controle se mexeu ou está pressionado; o teclado tem prioridade
        _useMouse = (moved || GameInput.PointerHeld()) && Mathf.Abs(_h) < 0.01f;
        if (_useMouse && _camera != null)
            _targetX = _camera.ScreenToWorldPoint(new Vector3(p.x, p.y, -_camera.transform.position.z)).x;
    }

    void FixedUpdate()
    {
        if (_useMouse)                   _playerController.MoveTo(_targetX);
        else if (Mathf.Abs(_h) > 0.01f)  _playerController.Move(_h);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.Play(Sfx.Player);
        StopAllCoroutines();
        StartCoroutine(Squash());
    }

    private IEnumerator Squash()
    {
        const float duration = 0.12f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            transform.localScale = new Vector3(_baseScale.x * (1f + 0.08f * k), _baseScale.y * (1f - 0.22f * k), _baseScale.z);
            yield return null;
        }
        transform.localScale = _baseScale;
    }
}
