using UnityEngine;

public class SteelBrickModel : BrickModel
{
    public override int    MaxHealth   => 3;
    public override string DisplayName => "Aço";

    public override int PointsPerHit => ScoreRules.DamageHit + 1;

    public override Color GetColor(float healthRatio)
    {
        float k = Mathf.Lerp(1.05f, 0.62f, healthRatio);
        Color steel = new Color(0.62f, 0.66f, 0.72f, 1f);
        return new Color(steel.r * k, steel.g * k, steel.b * k, 1f);
    }
}
