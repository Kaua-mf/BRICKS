using UnityEngine;


public abstract class BrickModel : MonoBehaviour
{
    [SerializeField] private float _health;
    [SerializeField] private Color _tint = Color.white;

    
    public abstract int MaxHealth { get; }

    public abstract string DisplayName { get; }

    public abstract Color GetColor(float healthRatio);

    public virtual int BasePoints => ScoreRules.BrickDestroyed;

    public virtual int BonusPerExtraLife => ScoreRules.ExtraPointsPerExtraLife;

    public virtual int PointsPerHit => MaxHealth > 1 ? ScoreRules.DamageHit : 0;

    public int PointsOnDestroy => BasePoints + BonusPerExtraLife * (MaxHealth - 1);

    public float Health { get => _health; set => _health = value; }
    public Color Tint   { get => _tint;   set => _tint = value; }

    protected virtual void Awake()
    {
        _health = MaxHealth;
    }
}
