using UnityEngine;

/// <summary>Brick comum: 1 vida, 10 pontos, usa a cor sorteada da paleta.</summary>
public class NormalBrickModel : BrickModel
{
    public override int    MaxHealth   => 1;
    public override string DisplayName => "Normal";

    public override Color GetColor(float healthRatio)
    {
        return Tint;
    }
}
