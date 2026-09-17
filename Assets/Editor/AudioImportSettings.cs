using UnityEditor;
using UnityEngine;

namespace AnimalGrid.Editor
{
    /// <summary>
    /// Auto-configures audio dropped into Assets/Resources/Audio:
    /// music = streaming Vorbis, SFX = decompress on load.
    /// </summary>
    public class AudioImportSettings : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/Resources/Audio/")) return;

            var importer = (AudioImporter)assetImporter;
            bool isMusic = assetPath.Contains("music_");
            var settings = importer.defaultSampleSettings;

            if (isMusic)
            {
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.5f;
            }
            else
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 1f;
            }

            importer.defaultSampleSettings = settings;
        }
    }
}