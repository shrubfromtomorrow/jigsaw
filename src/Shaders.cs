using System.IO;
using RWCustom;
using UnityEngine;

namespace Jigsaw
{
    public static class Shaders
    {
        public static FShader PuzzleGrab;
        public static FShader PuzzlePiece;
        public static FShader PuzzlePieceOutline;
        public static FShader PuzzlePieceColor;

        internal static void LoadShaders()
        {
            var path = AssetManager.ResolveFilePath(Path.Combine("shaders", "jigsaw"));
            var bundle = AssetBundle.LoadFromFile(path);
            PuzzleGrab = FShader.CreateShader("PuzzleGrab", bundle.LoadAsset<Shader>("Assets/Shaders/PuzzleGrab.shader"));
            PuzzlePiece = FShader.CreateShader("PuzzlePiece", bundle.LoadAsset<Shader>("Assets/Shaders/PuzzlePiece.shader"));
            PuzzlePieceOutline = FShader.CreateShader("PuzzlePieceOutline", bundle.LoadAsset<Shader>("Assets/Shaders/PuzzlePieceOutline.shader"));
            PuzzlePieceColor = FShader.CreateShader("PuzzlePieceColor", bundle.LoadAsset<Shader>("Assets/Shaders/PuzzlePieceColor.shader"));

            Custom.rainWorld.Shaders[nameof(PuzzleGrab)] = PuzzleGrab;
            Custom.rainWorld.Shaders[nameof(PuzzlePiece)] = PuzzlePiece;
            Custom.rainWorld.Shaders[nameof(PuzzlePieceOutline)] = PuzzlePieceOutline;
            Custom.rainWorld.Shaders[nameof(PuzzlePieceColor)] = PuzzlePieceColor;

            // Initial values for testing
            Shader.SetGlobalVector("_PuzzleSize", new Vector2(7, 5));
            Shader.SetGlobalVector("_PuzzleSeed", new Vector4(Random.value, Random.value, Random.value, Random.value));
        }
    }
}
