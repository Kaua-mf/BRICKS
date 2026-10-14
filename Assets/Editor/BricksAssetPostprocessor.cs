using System.IO;
using UnityEditor;
using UnityEngine;


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
