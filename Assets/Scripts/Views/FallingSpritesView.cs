using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FallingSpritesView : MonoBehaviour
{
    [SerializeField] private RectTransform _area;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Color[]  _colors;
    [SerializeField] private int      _count = 26;
    [SerializeField] private Vector2  _sizeRange  = new Vector2(24f, 56f);
    [SerializeField] private Vector2  _speedRange = new Vector2(70f, 190f);
    [SerializeField] private bool     _rise;

    private class Item
    {
        public RectTransform rt;
        public float speed, spin, sway, phase, baseX;
    }

    private readonly List<Item> _items = new List<Item>();

    void Start()
    {
        if (_area == null || _sprites == null || _sprites.Length == 0) return;
        Rect r = _area.rect;

        for (int i = 0; i < _count; i++)
        {
            GameObject go = new GameObject("Particle", typeof(RectTransform));
            go.transform.SetParent(_area, false);
            Image img = go.AddComponent<Image>();
            img.sprite        = _sprites[Random.Range(0, _sprites.Length)];
            img.raycastTarget = false;
            img.color         = (_colors != null && _colors.Length > 0) ? _colors[Random.Range(0, _colors.Length)] : Color.white;

            float size = Random.Range(_sizeRange.x, _sizeRange.y);
            Item it = new Item
            {
                rt    = go.GetComponent<RectTransform>(),
                speed = Random.Range(_speedRange.x, _speedRange.y),
                spin  = Random.Range(-120f, 120f),
                sway  = Random.Range(0.6f, 1.8f),
                phase = Random.Range(0f, 6.28f),
                baseX = Random.Range(r.xMin, r.xMax)
            };
            it.rt.sizeDelta        = new Vector2(size, size * (img.sprite.rect.height / img.sprite.rect.width));
            it.rt.anchoredPosition = new Vector2(it.baseX, Random.Range(r.yMin, r.yMax));
            _items.Add(it);
        }
    }

    void Update()
    {
        if (_items.Count == 0) return;
        Rect r = _area.rect;
        float dir = _rise ? 1f : -1f;

        foreach (Item it in _items)
        {
            Vector2 p = it.rt.anchoredPosition;
            p.y += dir * it.speed * Time.deltaTime;
            p.x  = it.baseX + Mathf.Sin(Time.time * it.sway + it.phase) * 24f;

            if (!_rise && p.y < r.yMin - 60f) p.y = r.yMax + 60f;
            if (_rise  && p.y > r.yMax + 60f) p.y = r.yMin - 60f;

            it.rt.anchoredPosition = p;
            it.rt.Rotate(0f, 0f, it.spin * Time.deltaTime);
        }
    }
}
