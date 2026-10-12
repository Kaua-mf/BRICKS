/// <summary>
/// Critérios de pontuação do jogo (um único lugar para ajustar as regras).
/// </summary>
public static class ScoreRules
{
    /// <summary>Pontos por demolir um brick (valor base).</summary>
    public const int BrickDestroyed = 10;

    /// <summary>Pontos A MAIS para cada vida extra do brick (brick com 3 vidas = 10 + 2x10 = 30).</summary>
    public const int ExtraPointsPerExtraLife = 10;

    /// <summary>Pontos por acertar (sem destruir) um brick que ainda tem vidas.</summary>
    public const int DamageHit = 2;

    /// <summary>Penalidade quando a bola bate no player.</summary>
    public const int PlayerHit = -5;

    /// <summary>Bônus por concluir uma fase.</summary>
    public const int LevelCompleted = 100;

    /// <summary>Bônus por vida restante ao vencer o jogo.</summary>
    public const int LifeBonus = 50;
}
