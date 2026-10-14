# Bricks — Atividade Avaliativa I (IFSC Tubarão)

Jogo estilo Brick Breaker em Unity 6 (2D), feito em cima do projeto da aula (**MVC**: Model / View / Controller).

## Controles
- **A / D**, setas ou **mouse/toque**: mover a raquete
- **Espaço** ou **clique**: lançar a bola
- **M**: liga/desliga o som · **Enter/R**: recomeçar (telas finais) · **Esc**: menu

## Onde está cada critério da atividade

| Critério | Onde |
|---|---|
| Áudio: música de fundo | `Core/AudioManager.cs` troca a música por cena (menu, fases, vitória, game over). Arquivos em `Resources/Audio/bgm_*.wav` |
| Áudio: efeitos de colisão (ball, player, bricks) | Ball: `BallView` (parede e lançamento) · Player: `PlayerView` · Bricks: `BrickView` (acerto e destruição) |
| Abstração de objetos (bricks) | `Models/BrickModel.cs` (**classe abstrata**) + `NormalBrickModel`, `StrongBrickModel`, `SteelBrickModel`. `BrickController` só conhece a abstração (polimorfismo) |
| Vidas (3) com demonstrativo em tela | Dados: `GameSession.Lives` · Regra: `LevelController` · Tela: `Views/HudView.cs` (3 corações) |
| Independência de camadas | Views só **escutam eventos** (`GameSession.ScoreChanged/LivesChanged`, `LevelController.StateChanged`, `BrickController.Damaged`...) e nunca alteram dados. Models não conhecem Views nem Controllers |
| Pontuação por ação | `Core/ScoreRules.cs` (critérios) aplicados pelos Controllers: brick 10 · brick de 2 vidas 20 · de 3 vidas 30 · acerto sem destruir +2/+3 · **bater no player −5** · fase concluída +100 · bônus de 50 por vida restante na vitória |
| Singleton (pontuação, nome do jogador...) | `Core/GameSession.cs` — pontos, nome, vidas, fase atual, recorde. Sobrevive às trocas de cena |
| Tela (scene) de abertura | `Scenes/Menu` (nome do jogador, Jogar, Sair, recorde) — `Views/MenuView.cs` |
| Recomeçar no game over | Botão **RECOMEÇAR** (ou Enter/R) em `Scenes/GameOver` → `GameFlowController.RestartGame()` zera pontos e vidas |
| Mais duas fases (scenes) | `Fase1`, `Fase2`, `Fase3` — grades diferentes, tijolos reforçados/aço, bola mais rápida e fundo diferente |
| Ação de vitória | Concluir a Fase 3 → cena `Vitoria` (confete, pontuação animada, recorde) |
| Ação de game over | Perder todas as vidas → cena `GameOver` |
| Visual | Sprites com bisel, bola metálica com rastro, raquete com pontas vermelhas, rachaduras nos tijolos, estilhaços, tremor de câmera, HUD com corações, logos, fundos de espaço por fase, confete |

## Arquitetura (MVC)

```
Core/          GameSession (Singleton) · ScoreRules · AudioManager · GameInput · SceneNames
Models/        BallModel · PlayerModel · BrickModel (abstrata) + Normal/Strong/Steel · LevelModel
Controllers/   BallController · PlayerController · BrickController · LevelController · GameFlowController
Views/         BallView · PlayerView · BrickView · HudView · LevelView · MenuView · EndScreenView + efeitos
Editor/        BricksProjectBuilder (gera cenas/prefabs) · BricksAssetPostprocessor (importa sprites)
```

Fluxo de uma colisão bola × brick: `BallView.OnCollisionEnter2D` → `BrickView.PerformTakeDamage` → `BrickController.TakeDamage`
→ (`GameSession.AddScore`) → evento `Damaged/Destroying` → `BrickView` toca o som/estilhaços → `HudView` atualiza a pontuação ao escutar `ScoreChanged`.
