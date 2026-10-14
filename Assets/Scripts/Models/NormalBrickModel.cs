using UnityEngine;

public class NormalBrickModel : BrickModel
{
    public override int    MaxHealth   => 1;
    public override string DisplayName => "Normal";

    public override Color GetColor(float healthRatio)
    {
        return Tint;
    }
}
