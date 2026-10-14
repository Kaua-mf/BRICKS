using UnityEngine;

public class StrongBrickModel : BrickModel
{
    private static readonly Color Gold = new Color(1f, 0.82f, 0.12f, 1f);

    public override int    MaxHealth   => 2;
    public override string DisplayName => "Reforçado";

    public override Color GetColor(float healthRatio)
    {
        float k = Mathf.Lerp(0.78f, 1f, healthRatio);
        return new Color(Gold.r * k, Gold.g * k, Gold.b * k, 1f);
    }
}
