using UnityEngine;

/// <summary>
/// ABSTRAÇÃO DE BRICKS: classe base abstrata do MODEL dos bricks.
/// Define o que todo brick tem (vida, cor, pontos) e OBRIGA cada tipo concreto
/// (NormalBrickModel, StrongBrickModel, SteelBrickModel) a informar sua própria
/// resistência, pontuação e cor (polimorfismo).
/// </summary>
public abstract class BrickModel : MonoBehaviour
{
    [SerializeField] private float _health;
    [SerializeField] private Color _tint = Color.white;

    // ---------------------------------------------- membros abstratos (cada tipo define)
    /// <summary>Quantidade de vidas do brick.</summary>
    public abstract int MaxHealth { get; }

    /// <summary>Nome do tipo (para depuração/UI).</summary>
    public abstract string DisplayName { get; }

    /// <summary>Cor do brick conforme a vida restante (1 = inteiro, 0 = quase destruído).</summary>
    public abstract Color GetColor(float healthRatio);

    // ---------------------------------------------- membros virtuais (podem ser sobrescritos)
    /// <summary>Pontos base por demolir o brick.</summary>
    public virtual int BasePoints => ScoreRules.BrickDestroyed;

    /// <summary>Pontos a mais por cada vida extra ("bricks com mais de uma vida valem x pontos a mais").</summary>
    public virtual int BonusPerExtraLife => ScoreRules.ExtraPointsPerExtraLife;

    /// <summary>Pontos ao acertar o brick sem destruí-lo.</summary>
    public virtual int PointsPerHit => MaxHealth > 1 ? ScoreRules.DamageHit : 0;

    // ---------------------------------------------- dados concretos
    /// <summary>Total de pontos ao destruir: base + bônus por vida extra.</summary>
    public int PointsOnDestroy => BasePoints + BonusPerExtraLife * (MaxHealth - 1);

    public float Health { get => _health; set => _health = value; }
    public Color Tint   { get => _tint;   set => _tint = value; }

    protected virtual void Awake()
    {
        _health = MaxHealth;
    }
}
