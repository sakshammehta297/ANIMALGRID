using UnityEditor;
using UnityEngine;

namespace AnimalGrid.Editor
{
    /// <summary>
    /// Auto-configures textures dropped into Assets/Resources/Art as sprites.
    /// </summary>
    public class ArtImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/Resources/Art/"))
            {
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
    }
}