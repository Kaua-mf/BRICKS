using UnityEngine;

/// <summary>Estilhaços coloridos que voam quando um brick é destruído (efeito visual).</summary>
public class DebrisView : MonoBehaviour
{
    private static Sprite _sprite;

    private Vector2 _velocity;
    private float _life, _maxLife, _spin;
    private SpriteRenderer _renderer;
    private Color _color;

    private static Sprite GetSprite()
    {
        if (_sprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            _sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        }
        return _sprite;
    }

    public static void Burst(Vector3 position, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Debris");
            go.transform.position = position;
            float size = Random.Range(0.06f, 0.13f);
            go.transform.localScale = new Vector3(size, size, 1f);

            DebrisView d = go.AddComponent<DebrisView>();
            d._renderer = go.AddComponent<SpriteRenderer>();
            d._renderer.sprite = GetSprite();
            d._renderer.sortingOrder = 8;
            d._color    = color;
            d._velocity = Random.insideUnitCircle * 3.2f + Vector2.up * 1.6f;
            d._spin     = Random.Range(-360f, 360f);
            d._maxLife  = d._life = Random.Range(0.35f, 0.6f);
        }
    }

    void Update()
    {
        _life -= Time.deltaTime;
        if (_life <= 0f) { Destroy(gameObject); return; }

        _velocity.y -= 9f * Time.deltaTime;
        transform.position += (Vector3)(_velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, _spin * Time.deltaTime);

        Color c = _color;
        c.a = _life / _maxLife;
        _renderer.color = c;
    }
}
