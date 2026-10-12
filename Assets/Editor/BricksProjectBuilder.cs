using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gera o projeto completo com UM clique: Menu  Bricks > Gerar jogo completo.
/// Cria as tags, o material de física, os prefabs (player, bola, 3 tipos de brick) e as 6 cenas
/// (Menu, Fase1, Fase2, Fase3, Vitoria, GameOver), liga todas as referências e registra no Build Settings.
/// </summary>
public static class BricksProjectBuilder
{
    // ------------------------------------------------------------------ pastas
    private const string ArtDir     = "Assets/Art/";
    private const string PrefabDir  = "Assets/Prefabs/";
    private const string SceneDir   = "Assets/Scenes/";
    private const string PhysicsDir = "Assets/Physics/";

    // ------------------------------------------------------------------- mundo (6 x 10 unidades)
    private const float HalfW      = 3f;
    private const float TopInner   = 4.2f;
    private const float BrickW     = 0.75f;
    private const float BrickH     = 0.30f;
    private const float BricksTop  = 3.7f;
    private const float PlayerY    = -4.2f;
    private const float CanvasW    = 1080f;
    private const float CanvasH    = 1800f;

    private static readonly Color[] Palette =
    {
        new Color(0.95f, 0.25f, 0.75f), new Color(0.25f, 0.50f, 1.00f), new Color(0.95f, 0.20f, 0.25f),
        new Color(0.60f, 0.30f, 0.90f), new Color(0.35f, 0.85f, 0.25f), new Color(0.20f, 0.85f, 0.85f),
        new Color(1.00f, 0.65f, 0.15f)
    };

    // N = normal (1 vida) | S = reforçado (2 vidas) | T = aço (3 vidas) | . = vazio  (8 colunas)
    private static readonly string[] Layout1 =
    {
        "SSNNNNSS",
        "NNNNNNNN",
        "NNSSSSNN",
        "NNNNNNNN",
        "SNNNNNNS",
        "NNNNNNNN"
    };

    private static readonly string[] Layout2 =
    {
        "SSSSSSSS",
        "NNNNNNNN",
        "N.N.N.N.",
        ".N.N.N.N",
        "NNNNNNNN",
        "SSSSSSSS"
    };

    private static readonly string[] Layout3 =
    {
        "TSNNNNST",
        "STSNNSTS",
        "NSTSSTSN",
        ".NSTTSN.",
        "..NSSN..",
        "...NN..."
    };

    // =============================================================== PONTO DE ENTRADA
    [MenuItem("Bricks/Gerar jogo completo (tags, prefabs e cenas)")]
    public static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("Bricks",
                "Isto vai (re)criar os prefabs e as cenas Menu, Fase1, Fase2, Fase3, Vitoria e GameOver em Assets/. Continuar?",
                "Gerar", "Cancelar")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        AssetDatabase.Refresh();
        if (!EnsureArt()) return;   // localiza/move/importa os sprites; aborta com mensagem clara se faltar algo
        ConfigureArt();

        EnsureTag("Enemy");
        EnsureTag("Wall");
        EnsureTag("DeadZone");

        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Physics");

        PhysicsMaterial2D bounce = CreatePhysicsMaterial();

        GameObject playerPrefab = CreatePlayerPrefab(bounce);
        GameObject ballPrefab   = CreateBallPrefab(bounce);
        GameObject normalBrick  = CreateBrickPrefab("NormalBrick", "brick.png",        typeof(NormalBrickModel), bounce);
        GameObject strongBrick  = CreateBrickPrefab("StrongBrick", "brick_strong.png", typeof(StrongBrickModel), bounce);
        GameObject steelBrick   = CreateBrickPrefab("SteelBrick",  "brick_steel.png",  typeof(SteelBrickModel),  bounce);
        GameObject[] bricks = { normalBrick, strongBrick, steelBrick };

        BuildMenuScene();
        BuildLevelScene(1, "bg_fase1.png", Layout1, 6.0f, SceneNames.Fase2,   playerPrefab, ballPrefab, bricks, bounce);
        BuildLevelScene(2, "bg_fase2.png", Layout2, 7.0f, SceneNames.Fase3,   playerPrefab, ballPrefab, bricks, bounce);
        BuildLevelScene(3, "bg_fase3.png", Layout3, 8.0f, SceneNames.Victory, playerPrefab, ballPrefab, bricks, bounce);
        BuildEndScene(true);
        BuildEndScene(false);

        RegisterScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(SceneDir + SceneNames.Menu + ".unity");
        EditorUtility.DisplayDialog("Bricks",
            "Pronto! 6 cenas criadas e registradas no Build Settings.\n\nAbra a cena Menu e aperte Play.\n" +
            "Dica: na aba Game escolha a proporção 9:16 (Portrait).", "OK");
    }

    // ================================================================== PREPARAÇÃO
    private static readonly string[] RequiredArt =
    {
        "ball.png", "bg_fase1.png", "bg_fase2.png", "bg_fase3.png", "bg_gameover.png", "bg_menu.png",
        "bg_victory.png", "brick.png", "brick_steel.png", "brick_strong.png", "button.png", "confetti.png",
        "crack_1.png", "crack_2.png", "heart.png", "heart_empty.png", "logo_bricks.png", "paddle.png",
        "panel.png", "px.png", "star.png", "title_gameover.png", "title_victory.png"
    };

    /// <summary>
    /// Garante que todas as imagens existam em Assets/Art e estejam importadas como Sprite.
    /// Se a pasta foi extraída no lugar errado (ex.: Assets/Bricks_Para_Extrair_em_Assets/Art), move os arquivos.
    /// </summary>
    private static bool EnsureArt()
    {
        EnsureFolder("Assets", "Art");
        System.Collections.Generic.List<string> missing = new System.Collections.Generic.List<string>();

        foreach (string file in RequiredArt)
        {
            string target = ArtDir + file;
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(target) == null)
            {
                // procura o arquivo em qualquer outra pasta do projeto
                string found = null;
                string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(file);
                foreach (string guid in AssetDatabase.FindAssets(nameNoExt))
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    if (System.IO.Path.GetFileName(p) == file && p != target) { found = p; break; }
                }

                if (found == null) { missing.Add(file); continue; }

                string err = AssetDatabase.MoveAsset(found, target);
                if (!string.IsNullOrEmpty(err)) { missing.Add(file + " (falha ao mover de " + found + ": " + err + ")"); continue; }
                Debug.Log("Bricks: " + file + " estava em '" + found + "' e foi movido para " + target);
            }

            // força a importação como Sprite (caso o postprocessor ainda não estivesse compilado na 1a importação)
            TextureImporter importer = AssetImporter.GetAtPath(target) as TextureImporter;
            if (importer != null)
            {
                BricksAssetPostprocessor.Configure(importer, target);
                importer.SaveAndReimport();
            }
            if (AssetDatabase.LoadAssetAtPath<Sprite>(target) == null)
                missing.Add(file + " (existe, mas não foi importado como Sprite)");
        }

        if (missing.Count == 0) return true;

        string list = string.Join("\n - ", missing);
        Debug.LogError("Bricks: faltam imagens em Assets/Art:\n - " + list);
        EditorUtility.DisplayDialog("Bricks",
            "Não encontrei estas imagens em Assets/Art:\n\n - " + list +
            "\n\nExtraia o conteúdo do .zip DIRETAMENTE dentro da pasta Assets (devem ficar Assets/Art, Assets/Scripts, " +
            "Assets/Editor e Assets/Resources) e rode o menu de novo.", "OK");
        return false;
    }

    private static void ConfigureArt()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            BricksAssetPostprocessor.Configure(importer, path);
            importer.SaveAndReimport();
        }
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static PhysicsMaterial2D CreatePhysicsMaterial()
    {
        string path = PhysicsDir + "BallBounce.physicsMaterial2D";
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("BallBounce");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.bounciness = 1f;   // rebate sem perder energia
        mat.friction   = 0f;   // sem atrito
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ===================================================================== PREFABS
    private static GameObject SavePrefab(GameObject go, string name)
    {
        GameObject asset = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
        Object.DestroyImmediate(go);
        return asset;
    }

    private static GameObject CreatePlayerPrefab(PhysicsMaterial2D mat)
    {
        GameObject go = new GameObject("Player");
        go.tag = "Player";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("paddle.png");
        sr.sortingOrder = 10;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType      = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size           = sr.sprite.bounds.size;
        col.sharedMaterial = mat;

        go.AddComponent<PlayerModel>();
        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerView>();
        return SavePrefab(go, "Player");
    }

    private static GameObject CreateBallPrefab(PhysicsMaterial2D mat)
    {
        GameObject go = new GameObject("Ball");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("ball.png");
        sr.sortingOrder = 11;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType               = RigidbodyType2D.Dynamic;
        rb.gravityScale           = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation          = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode              = RigidbodySleepMode2D.NeverSleep;
        rb.constraints            = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius         = sr.sprite.bounds.extents.x;
        col.sharedMaterial = mat;

        go.AddComponent<BallModel>();
        go.AddComponent<BallController>();
        go.AddComponent<BallView>();
        return SavePrefab(go, "Ball");
    }

    private static GameObject CreateBrickPrefab(string name, string spriteFile, System.Type modelType, PhysicsMaterial2D mat)
    {
        GameObject go = new GameObject(name);
        go.tag = "Enemy";

        SpriteRenderer body = go.AddComponent<SpriteRenderer>();
        body.sprite       = Spr(spriteFile);
        body.sortingOrder = 5;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size           = body.sprite.bounds.size;
        col.sharedMaterial = mat;

        // Rachaduras (aparecem quando o brick leva dano)
        GameObject crackGo = new GameObject("Crack");
        crackGo.transform.SetParent(go.transform, false);
        SpriteRenderer crack = crackGo.AddComponent<SpriteRenderer>();
        crack.sortingOrder = 6;
        crack.enabled      = false;

        go.AddComponent(modelType);            // NormalBrickModel / StrongBrickModel / SteelBrickModel
        go.AddComponent<BrickController>();
        BrickView view = go.AddComponent<BrickView>();

        SetRef(view, "_body", body);
        SetRef(view, "_crack", crack);
        SetRefs(view, "_crackSprites", new Object[] { Spr("crack_1.png"), Spr("crack_2.png") });
        return SavePrefab(go, name);
    }

    // ======================================================================= CENAS
    private static Scene NewScene()
    {
        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static void SaveScene(Scene scene, string name)
    {
        EditorSceneManager.SaveScene(scene, SceneDir + name + ".unity");
    }

    private static void RegisterScenes()
    {
        string[] names =
        {
            SceneNames.Menu, SceneNames.Fase1, SceneNames.Fase2, SceneNames.Fase3,
            SceneNames.Victory, SceneNames.GameOver
        };
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[names.Length];
        for (int i = 0; i < names.Length; i++)
            scenes[i] = new EditorBuildSettingsScene(SceneDir + names[i] + ".unity", true);
        EditorBuildSettings.scenes = scenes;
    }

    // -------------------------------------------------------------------- Menu
    private static void BuildMenuScene()
    {
        Scene scene = NewScene();
        Camera cam = CreateCamera();
        CreateBackground("bg_menu.png");

        Canvas canvas = CreateCanvas("UI", cam, 100);
        Transform ui = canvas.transform;

        // Fundo 100% estático: sem estrelas/bricks caindo.

        Image logo = MakeImage(ui, "Logo", Spr("logo_bricks.png"), Color.white, new Vector2(0.5f, 0.5f),
                               new Vector2(0f, 460f), new Vector2(940f, 427f));

        MakeText(ui, "NameLabel", "NOME DO JOGADOR", 34, new Color(0.85f, 0.8f, 1f), TextAlignmentOptions.Center,
                 new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(700f, 50f));
        TMP_InputField input = MakeInputField(ui, "NameInput", new Vector2(0f, 60f), new Vector2(700f, 110f));

        Button play = MakeButton(ui, "PlayButton", "JOGAR", new Color(0.25f, 0.80f, 0.35f),
                                 new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(560f, 140f));
        Button quit = MakeButton(ui, "QuitButton", "SAIR", new Color(0.90f, 0.30f, 0.35f),
                                 new Vector2(0.5f, 0.5f), new Vector2(0f, -290f), new Vector2(400f, 110f));

        TextMeshProUGUI high = MakeText(ui, "HighScore", "RECORDE: 0", 46, new Color(1f, 0.9f, 0.4f), TextAlignmentOptions.Center,
                                        new Vector2(0.5f, 0.5f), new Vector2(0f, -450f), new Vector2(800f, 70f));
        MakeHint(ui, "A / D, setas ou mouse: mover   |   ESPAÇO ou clique: lançar\nM: liga/desliga o som", 100f);

        GameObject manager = new GameObject("MenuManager");
        GameFlowController flow = manager.AddComponent<GameFlowController>();
        MenuView view = manager.AddComponent<MenuView>();
        SetRef(view, "_flow", flow);
        SetRef(view, "_nameInput", input);
        SetRef(view, "_playButton", play);
        SetRef(view, "_quitButton", quit);
        SetRef(view, "_highScoreText", high);
        SetRef(view, "_logo", logo.rectTransform);

        CreateEventSystem();
        SaveScene(scene, SceneNames.Menu);
    }

    // ------------------------------------------------------------------- Fases
    private static void BuildLevelScene(int number, string background, string[] layout, float ballSpeed, string nextScene,
                                        GameObject playerPrefab, GameObject ballPrefab, GameObject[] brickPrefabs,
                                        PhysicsMaterial2D bounce)
    {
        Scene scene = NewScene();
        Camera cam = CreateCamera();
        CreateBackground(background);
        CreateWalls(bounce);
        CreateEdgeGlow();

        // Player
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = new Vector3(0f, PlayerY, 0f);

        // Bola (o LevelController gruda no player ao iniciar)
        GameObject ball = (GameObject)PrefabUtility.InstantiatePrefab(ballPrefab);
        ball.transform.position = new Vector3(0f, PlayerY + 0.30f, 0f);
        SetFloat(ball.GetComponent<BallModel>(), "_speed", ballSpeed);

        // Bricks
        Transform parent = new GameObject("Bricks").transform;
        for (int r = 0; r < layout.Length; r++)
        {
            for (int c = 0; c < layout[r].Length; c++)
            {
                char ch = layout[r][c];
                int type = ch == 'N' ? 0 : ch == 'S' ? 1 : ch == 'T' ? 2 : -1;
                if (type < 0) continue;

                GameObject brick = (GameObject)PrefabUtility.InstantiatePrefab(brickPrefabs[type], parent);
                brick.transform.position = new Vector3(-HalfW + (c + 0.5f) * BrickW, BricksTop - (r + 0.5f) * BrickH, 0f);
                SetColor(brick.GetComponent<BrickModel>(), "_tint", Palette[(r + c) % Palette.Length]);
            }
        }

        // Gerenciador da fase (Model + Controller + View)
        GameObject manager = new GameObject("LevelManager");
        LevelModel model = manager.AddComponent<LevelModel>();
        LevelController controller = manager.AddComponent<LevelController>();
        LevelView levelView = manager.AddComponent<LevelView>();

        SetInt(model, "_levelNumber", number);
        SetString(model, "_levelTitle", "Fase " + number);
        SetString(model, "_nextScene", nextScene);
        SetRef(controller, "_ball", ball.GetComponent<BallController>());
        SetRef(controller, "_player", player.GetComponent<PlayerController>());
        SetRef(levelView, "_level", controller);
        SetRef(levelView, "_camera", cam);

        BuildHud(cam, controller);

        SaveScene(scene, "Fase" + number);
    }

    private static void BuildHud(Camera cam, LevelController level)
    {
        Canvas canvas = CreateCanvas("HUD", cam, 100);
        Transform root = canvas.transform;

        // Barra superior
        Image bar = MakeImage(root, "TopBar", Spr("px.png"), new Color(0.03f, 0.03f, 0.035f, 0.80f),
                              new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 150f), false, false);
        RectTransform barRt = bar.rectTransform;
        barRt.anchorMin = new Vector2(0f, 1f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot     = new Vector2(0.5f, 1f);
        barRt.anchoredPosition = Vector2.zero;
        barRt.sizeDelta        = new Vector2(0f, 150f);

        Image line = MakeImage(root, "TopLine", Spr("px.png"), new Color(0.85f, 0.85f, 0.88f, 0.9f),
                               new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(0f, 4f), false, false);
        line.rectTransform.anchorMin = new Vector2(0f, 1f);
        line.rectTransform.anchorMax = new Vector2(1f, 1f);
        line.rectTransform.sizeDelta = new Vector2(0f, 4f);

        TextMeshProUGUI nameText = MakeText(bar.transform, "NameText", "JOGADOR", 32, new Color(0.75f, 0.85f, 1f),
                                            TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(28f, -14f), new Vector2(460f, 44f));
        TextMeshProUGUI scoreText = MakeText(bar.transform, "ScoreText", "PONTOS  0", 54, Color.white,
                                             TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(28f, -58f), new Vector2(470f, 80f));
        TextMeshProUGUI levelText = MakeText(bar.transform, "LevelText", "FASE 1", 52, new Color(1f, 0.9f, 0.4f),
                                             TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(110f, -40f), new Vector2(260f, 80f));

        // Corações (vidas)
        Image[] hearts = new Image[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = MakeImage(bar.transform, "Heart" + (i + 1), Spr("heart.png"), Color.white,
                                  new Vector2(1f, 1f), new Vector2(-28f - (hearts.Length - 1 - i) * 84f, -39f), new Vector2(72f, 72f));
        }

        // Mensagem central ("Clique para lançar", "Fase concluída"...)
        Image panel = MakeImage(root, "Message", Spr("panel.png"), new Color(0.03f, 0.03f, 0.035f, 0.80f),
                                new Vector2(0.5f, 0.5f), new Vector2(0f, -330f), new Vector2(900f, 160f), true);
        TextMeshProUGUI message = MakeText(panel.transform, "MessageText", "Clique ou ESPAÇO para lançar", 50, Color.white,
                                           TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 120f));

        HudView hud = canvas.gameObject.AddComponent<HudView>();
        SetRef(hud, "_level", level);
        SetRef(hud, "_nameText", nameText);
        SetRef(hud, "_scoreText", scoreText);
        SetRef(hud, "_levelText", levelText);
        SetRefs(hud, "_hearts", hearts);
        SetRef(hud, "_heartFull", Spr("heart.png"));
        SetRef(hud, "_heartEmpty", Spr("heart_empty.png"));
        SetRef(hud, "_messageRoot", panel.gameObject);
        SetRef(hud, "_messageText", message);
    }

    // ------------------------------------------------------ Vitória / Game Over
    private static void BuildEndScene(bool victory)
    {
        Scene scene = NewScene();
        Camera cam = CreateCamera();
        CreateBackground(victory ? "bg_victory.png" : "bg_gameover.png");

        Canvas canvas = CreateCanvas("UI", cam, 100);
        Transform ui = canvas.transform;

        // Fundo 100% estático: sem confete nem brasas.

        Image title = MakeImage(ui, "Title", Spr(victory ? "title_victory.png" : "title_gameover.png"), Color.white,
                                new Vector2(0.5f, 0.5f), new Vector2(0f, 560f), new Vector2(960f, 288f));

        Image panel = MakeImage(ui, "ResultPanel", Spr("panel.png"), new Color(0.03f, 0.03f, 0.035f, 0.82f),
                                new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(900f, 480f), true);
        Transform p = panel.transform;

        TextMeshProUGUI subtitle = MakeText(p, "Subtitle", "", 38, new Color(0.9f, 0.9f, 1f), TextAlignmentOptions.Center,
                                            new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(840f, 110f), FontStyles.Normal);
        MakeText(p, "ScoreLabel", "PONTUAÇÃO", 36, new Color(0.7f, 0.8f, 1f), TextAlignmentOptions.Center,
                 new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(600f, 50f));
        TextMeshProUGUI score = MakeText(p, "Score", "0", 130, new Color(1f, 0.92f, 0.4f), TextAlignmentOptions.Center,
                                         new Vector2(0.5f, 0.5f), new Vector2(0f, -5f), new Vector2(800f, 160f));
        TextMeshProUGUI high = MakeText(p, "HighScore", "Recorde: 0", 40, Color.white, TextAlignmentOptions.Center,
                                        new Vector2(0.5f, 0.5f), new Vector2(0f, -105f), new Vector2(700f, 60f));
        TextMeshProUGUI record = MakeText(p, "NewRecord", "NOVO RECORDE!", 56, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center,
                                          new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(800f, 80f));

        Button restart = MakeButton(ui, "RestartButton", victory ? "JOGAR NOVAMENTE" : "RECOMEÇAR", new Color(0.25f, 0.80f, 0.35f),
                                    new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(680f, 140f));
        Button menu = MakeButton(ui, "MenuButton", "MENU", new Color(0.30f, 0.55f, 0.95f),
                                 new Vector2(0.5f, 0.5f), new Vector2(0f, -420f), new Vector2(420f, 110f));
        Button quit = MakeButton(ui, "QuitButton", "SAIR", new Color(0.90f, 0.30f, 0.35f),
                                 new Vector2(0.5f, 0.5f), new Vector2(0f, -560f), new Vector2(320f, 90f));
        MakeHint(ui, "ENTER ou R: recomeçar   |   ESC: menu", 64f);

        GameObject manager = new GameObject("EndScreenManager");
        GameFlowController flow = manager.AddComponent<GameFlowController>();
        EndScreenView view = manager.AddComponent<EndScreenView>();
        SetRef(view, "_flow", flow);
        SetBool(view, "_isVictory", victory);
        SetRef(view, "_subtitleText", subtitle);
        SetRef(view, "_scoreText", score);
        SetRef(view, "_highScoreText", high);
        SetRef(view, "_recordRoot", record.gameObject);
        SetRef(view, "_title", title.rectTransform);
        SetRef(view, "_restartButton", restart);
        SetRef(view, "_menuButton", menu);
        SetRef(view, "_quitButton", quit);

        CreateEventSystem();
        SaveScene(scene, victory ? SceneNames.Victory : SceneNames.GameOver);
    }

    // ================================================================ OBJETOS DE CENA
    private static Camera CreateCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, 0f, -10f);

        Camera cam = go.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.02f, 0.02f, 0.025f);

        go.AddComponent<AudioListener>();
        go.AddComponent<CameraFitView>();
        return cam;
    }

    private static void CreateBackground(string file)
    {
        GameObject go = new GameObject("Background");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr(file);
        sr.sortingOrder = -10;

        Vector3 size = sr.sprite.bounds.size;
        float scale = Mathf.Max(2f * HalfW / size.x, 10f / size.y);   // cobre a área 6 x 10
        go.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static void CreateWalls(PhysicsMaterial2D bounce)
    {
        MakeWall("WallLeft",  new Vector2(-HalfW - 0.15f, 0f),          new Vector2(0.3f, 10.4f), bounce);
        MakeWall("WallRight", new Vector2( HalfW + 0.15f, 0f),          new Vector2(0.3f, 10.4f), bounce);
        MakeWall("WallTop",   new Vector2(0f, TopInner + 0.15f),        new Vector2(6.6f, 0.3f),  bounce);

        // Zona de perda (gatilho abaixo da tela)
        GameObject dead = new GameObject("DeadZone");
        dead.tag = "DeadZone";
        dead.transform.position = new Vector3(0f, -5.7f, 0f);
        BoxCollider2D col = dead.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(9f, 1f);
    }

    private static void MakeWall(string name, Vector2 position, Vector2 size, PhysicsMaterial2D bounce)
    {
        GameObject go = new GameObject(name);
        go.tag = "Wall";
        go.transform.position   = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("px.png");
        sr.color        = new Color(0.82f, 0.82f, 0.85f, 0.85f);
        sr.sortingOrder = 2;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size           = Vector2.one;
        col.sharedMaterial = bounce;
    }

    /// <summary>Moldura luminosa nas laterais da área de jogo (apenas visual, sem colisor).</summary>
    private static void CreateEdgeGlow()
    {
        foreach (float side in new[] { -1f, 1f })
        {
            MakeStrip("EdgeLine", new Vector2(side * 2.97f, -0.4f), new Vector2(0.05f, 9.2f), new Color(0.88f, 0.88f, 0.90f, 0.70f));
            MakeStrip("EdgeGlow", new Vector2(side * 2.86f, -0.4f), new Vector2(0.28f, 9.2f), new Color(0.88f, 0.88f, 0.92f, 0.10f));
        }
    }

    private static void MakeStrip(string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.position   = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("px.png");
        sr.color        = color;
        sr.sortingOrder = 12;
    }

    private static void CreateEventSystem()
    {
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    // =================================================================== UI (helpers)
    private static Canvas CreateCanvas(string name, Camera cam, int order)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera  = cam;
        canvas.planeDistance = 5f;
        canvas.sortingOrder = order;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(CanvasW, CanvasH);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static Image MakeImage(Transform parent, string name, Sprite sprite, Color color,
                                   Vector2 anchor, Vector2 position, Vector2 size, bool sliced = false, bool keepAspect = true)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite         = sprite;
        img.color          = color;
        img.type           = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.preserveAspect = keepAspect && !sliced;
        img.raycastTarget  = false;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta        = size;
        return img;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, string text, float fontSize, Color color,
                                            TextAlignmentOptions align, Vector2 anchor, Vector2 position, Vector2 box,
                                            FontStyles style = FontStyles.Bold)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text          = text;
        t.fontSize      = fontSize;
        t.color         = color;
        t.alignment     = align;
        t.fontStyle     = style;
        t.raycastTarget = false;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta        = box;
        return t;
    }

    private static Button MakeButton(Transform parent, string name, string label, Color color,
                                     Vector2 anchor, Vector2 position, Vector2 size)
    {
        Image img = MakeImage(parent, name, Spr("button.png"), color, anchor, position, size, true);
        img.raycastTarget = true;

        Button button = img.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        ColorBlock colors = button.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor     = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor    = Color.white;
        button.colors = colors;

        TextMeshProUGUI text = MakeText(img.transform, "Label", label, Mathf.Min(size.y * 0.42f, 56f), Color.white,
                                        TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        RectTransform rt = text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return button;
    }

    /// <summary>Texto de dica no rodapé, sobre uma "pílula" escura translúcida (legível em qualquer fundo).</summary>
    private static void MakeHint(Transform parent, string text, float height)
    {
        Image pill = MakeImage(parent, "Hint", Spr("panel.png"), new Color(0.02f, 0.02f, 0.025f, 0.55f),
                               new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(980f, height), true);
        TextMeshProUGUI t = MakeText(pill.transform, "HintText", text, 28, new Color(0.92f, 0.92f, 1f),
                                     TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, height),
                                     FontStyles.Normal);
        RectTransform rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static TMP_InputField MakeInputField(Transform parent, string name, Vector2 position, Vector2 size)
    {
        TMP_DefaultControls.Resources res = new TMP_DefaultControls.Resources();
        res.standard   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        res.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        res.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        res.knob       = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        res.checkmark  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        res.dropdown   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        res.mask       = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");

        GameObject go = TMP_DefaultControls.CreateInputField(res);
        go.name = name;
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta        = size;

        Image bg = go.GetComponent<Image>();
        bg.sprite = Spr("panel.png");
        bg.type   = Image.Type.Sliced;
        bg.color  = new Color(0.22f, 0.13f, 0.52f, 0.95f);

        TMP_InputField input = go.GetComponent<TMP_InputField>();
        input.characterLimit  = 14;
        input.customCaretColor = true;
        input.caretColor       = Color.white;
        input.caretWidth       = 4;

        input.textComponent.fontSize  = 50;
        input.textComponent.color     = Color.white;
        input.textComponent.alignment = TextAlignmentOptions.Center;

        TMP_Text placeholder = (TMP_Text)input.placeholder;
        placeholder.text      = "Digite seu nome...";
        placeholder.fontSize  = 44;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color     = new Color(0.7f, 0.7f, 0.9f, 0.7f);
        placeholder.alignment = TextAlignmentOptions.Center;
        return input;
    }

    private static FallingSpritesView MakeDecor(Transform parent, Sprite[] sprites, Color[] colors, int count,
                                                Vector2 sizeRange, Vector2 speedRange, bool rise)
    {
        GameObject go = new GameObject("Decor", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        FallingSpritesView view = go.AddComponent<FallingSpritesView>();
        SetRef(view, "_area", rt);
        SetRefs(view, "_sprites", sprites);
        SetColors(view, "_colors", colors);
        SetInt(view, "_count", count);
        SetVector2(view, "_sizeRange", sizeRange);
        SetVector2(view, "_speedRange", speedRange);
        SetBool(view, "_rise", rise);
        return view;
    }

    private static Color[] WithAlpha(Color[] source, float alpha)
    {
        Color[] result = new Color[source.Length];
        for (int i = 0; i < source.Length; i++)
            result[i] = new Color(source[i].r, source[i].g, source[i].b, alpha);
        return result;
    }

    // ======================================================== SerializedObject (helpers)
    private static Sprite Spr(string file)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + file);
        if (sprite == null)
        {
            // última tentativa: reimporta como Sprite
            TextureImporter importer = AssetImporter.GetAtPath(ArtDir + file) as TextureImporter;
            if (importer != null)
            {
                BricksAssetPostprocessor.Configure(importer, ArtDir + file);
                importer.SaveAndReimport();
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + file);
            }
        }
        if (sprite == null) Debug.LogError("Bricks: sprite não encontrado em " + ArtDir + file + " (confira a pasta Assets/Art).");
        return sprite;
    }

    private static SerializedProperty Prop(SerializedObject so, string field)
    {
        SerializedProperty p = so.FindProperty(field);
        if (p == null) Debug.LogError("Bricks: campo '" + field + "' não encontrado em " + so.targetObject.GetType().Name);
        return p;
    }

    private static void SetRef(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRefs(Object target, string field, Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRefs(Object target, string field, Image[] values)
    {
        Object[] objects = new Object[values.Length];
        for (int i = 0; i < values.Length; i++) objects[i] = values[i];
        SetRefs(target, field, objects);
    }

    private static void SetRefs(Object target, string field, Sprite[] values)
    {
        Object[] objects = new Object[values.Length];
        for (int i = 0; i < values.Length; i++) objects[i] = values[i];
        SetRefs(target, field, objects);
    }

    private static void SetColors(Object target, string field, Color[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).colorValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string field, float value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(Object target, string field, int value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Object target, string field, bool value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetColor(Object target, string field, Color value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.colorValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector2(Object target, string field, Vector2 value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.vector2Value = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
