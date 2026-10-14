using System.Collections;
using UnityEngine;


public class BrickView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _body;
    [SerializeField] private SpriteRenderer _crack;
    [SerializeField] private Sprite[]       _crackSprites;

    private BrickController _brickController;
    private BrickModel      _brickModel;
    private Vector3         _baseScale;

    void Awake()
    {
        _brickController = GetComponent<BrickController>();
        _brickModel      = GetComponent<BrickModel>();
        _baseScale       = transform.localScale;
    }

    void OnEnable()
    {
        _brickController.Damaged   += OnDamaged;
        _brickController.Destroying += OnDestroying;
    }

    void OnDisable()
    {
        _brickController.Damaged   -= OnDamaged;
        _brickController.Destroying -= OnDestroying;
    }

    void Start()
    {
        _body.color = _brickModel.GetColor(1f);
    }

    public void PerformTakeDamage(float damage)
    {
        _brickController.TakeDamage(damage);
    }

    private void OnDamaged(float healthRatio)
    {
        _body.color = _brickModel.GetColor(healthRatio);

        if (_crack != null && _crackSprites != null && _crackSprites.Length > 0)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt((1f - healthRatio) * _crackSprites.Length) - 1, 0, _crackSprites.Length - 1);
            _crack.sprite  = _crackSprites[index];
            _crack.enabled = true;
        }

        AudioManager.Play(Sfx.BrickHit, Random.Range(0.9f, 1.1f));
        StopAllCoroutines();
        StartCoroutine(Punch());
    }

    private void OnDestroying()
    {
        AudioManager.Play(Sfx.BrickBreak, Random.Range(0.92f, 1.08f));
        DebrisView.Burst(transform.position, _brickModel.GetColor(1f), 9);
    }

    private IEnumerator Punch()
    {
        const float duration = 0.14f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            transform.localScale = _baseScale * (1f + 0.12f * k);
            yield return null;
        }
        transform.localScale = _baseScale;
    }
}
