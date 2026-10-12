using UnityEngine;

/// <summary>Brick reforçado: 2 vidas, 20 pontos (10 base + 10 pelo bônus de vida extra). Cor fixa: DOURADO.</summary>
public class StrongBrickModel : BrickModel
{
    // Dourado (não existe na paleta dos bricks normais, então o reforçado é fácil de reconhecer)
    private static readonly Color Gold = new Color(1f, 0.82f, 0.12f, 1f);

    public override int    MaxHealth   => 2;
    public override string DisplayName => "Reforçado";

    public override Color GetColor(float healthRatio)
    {
        // Dourado brilhante enquanto está inteiro; escurece um pouco depois do primeiro acerto
        float k = Mathf.Lerp(0.78f, 1f, healthRatio);
        return new Color(Gold.r * k, Gold.g * k, Gold.b * k, 1f);
    }
}
