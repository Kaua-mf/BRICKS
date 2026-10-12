using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Importa automaticamente tudo que está em Assets/Art como Sprite, com a escala (pixels por unidade)
/// certa para cada imagem. Assim não é preciso configurar nada no Inspector.
/// </summary>
public class BricksAssetPostprocessor : AssetPostprocessor
{
    public const string ArtFolder = "Assets/Art/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtFolder)) return;
        Configure((TextureImporter)assetImporter, assetPath);
    }

    public static void Configure(TextureImporter importer, string path)
    {
        string n = Path.GetFileNameWithoutExtension(path);

        // Malha "Full Rect" (necessária para sprites 9-slice na UI)
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit(n);
        importer.spriteBorder        = Border(n);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled       = false;
        importer.filterMode          = FilterMode.Bilinear;
        importer.wrapMode            = TextureWrapMode.Clamp;
        importer.maxTextureSize      = 2048;
        importer.textureCompression  = n.StartsWith("bg_")
            ? TextureImporterCompression.Compressed
            : TextureImporterCompression.Uncompressed;
    }

    // brick 150x60 px -> 0.75 x 0.30 un | paddle 240x56 px -> 1.2 x 0.28 un (120 px / 100 da aula)
    // ball 128 px -> 0.30 un
    private static float PixelsPerUnit(string n)
    {
        if (n == "ball")                               return 128f / 0.3f;
        if (n.StartsWith("brick") || n.StartsWith("crack")) return 200f;
        if (n == "paddle")                             return 200f;
        if (n == "px")                                 return 4f;
        return 100f;
    }

    private static Vector4 Border(string n)
    {
        if (n == "button") return new Vector4(44f, 44f, 44f, 44f);
        if (n == "panel")  return new Vector4(40f, 40f, 40f, 40f);
        return Vector4.zero;
    }
}
