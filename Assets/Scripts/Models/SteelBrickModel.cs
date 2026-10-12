using UnityEngine;

/// <summary>Brick de aço: 3 vidas, 30 pontos (10 base + 2 x 10 de bônus) e bônus extra por acerto.</summary>
public class SteelBrickModel : BrickModel
{
    public override int    MaxHealth   => 3;
    public override string DisplayName => "Aço";

    // Sobrescreve um membro virtual: o aço paga mais por cada acerto
    public override int PointsPerHit => ScoreRules.DamageHit + 1;

    public override Color GetColor(float healthRatio)
    {
        float k = Mathf.Lerp(1.05f, 0.62f, healthRatio);
        Color steel = new Color(0.62f, 0.66f, 0.72f, 1f);
        return new Color(steel.r * k, steel.g * k, steel.b * k, 1f);
    }
}
