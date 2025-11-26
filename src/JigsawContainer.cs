using System;
using System.Collections.Generic;
using System.Linq;
using Jigsaw.PlayStyles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Jigsaw
{
    public sealed class JigsawContainer
    {
        public JigsawContainer(RainWorldGame game)
        {
            playStyleController = PlayStyleController.GetPlayStyleControllerFor(Options.SelectedPlayStyle, this, game);
        }

        public int Width => pieces?.GetLength(0) ?? 0;
        public int Height => pieces?.GetLength(1) ?? 0;
        public bool AllConnected => pieces == null || (pieces[0, 0].group.Count == Width * Height && heldPiece == null);

        public JigsawPiece[,] pieces = null;
        private readonly List<JigsawPiece> piecesList = [];
        public FContainer container = null;
        private FSprite backgroundSprite = null;
        private readonly PlayStyleController playStyleController;

        private JigsawPiece heldPiece = null;
        private Vector2 lastMousePos;
        private float flashCounter = 0;

        public void Regenerate()
        {
            ClearSprites();

            var (width, height) = Options.GetSize();

            pieces = new JigsawPiece[width, height];
            piecesList.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pieces[x, y] = new JigsawPiece(this, x, y);
                    piecesList.Add(pieces[x, y]);
                }
            }

            Shader.SetGlobalVector("_PuzzleSize", new Vector2(width, height));
            Shader.SetGlobalVector("_PuzzleSeed", new Vector4(Random.value, Random.value, Random.value, Random.value));
            InitiateSprites();
        }

        public void Regenerate(int seed)
        {
            var state = Random.state;
            Random.InitState(seed);
            Regenerate();
            Random.state = state;
        }

        private void ClearSprites()
        {
            if (container != null)
            {
                container.RemoveAllChildren();
                container.RemoveFromContainer();
                container = null;
            }
        }

        private void InitiateSprites()
        {
            // Base container
            container = new FContainer();
            Futile.stage.AddChild(container);
            container.SetPosition(0.01f, 0.01f);

            // Background
            backgroundSprite = new FSprite("Futile_White")
            {
                scaleX = Futile.screen.pixelWidth / 16f,
                scaleY = Futile.screen.pixelHeight / 16f,
                anchorX = 0,
                anchorY = 0,
                shader = Shaders.PuzzleGrab
            };
            container.AddChild(backgroundSprite);

            // Pieces
            foreach (var piece in pieces)
            {
                piece.InitializeSprite();
                container.AddChild(piece.sprite);
            }
        }

        public void Update(float dt)
        {
            //Futile.instance._cameraImage.texture.filterMode = FilterMode.Point;
            Futile.screen.renderTexture.filterMode = FilterMode.Point;
            container?.MoveToFront();

            Vector2 mousePos = Futile.mousePosition;

            if (container != null && pieces != null)
            {
                // Determine/move held piece
                JigsawPiece topPiece = null;
                if (heldPiece == null)
                {
                    int maxDepth = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            if (pieces[x, y].MouseIntersecting)
                            {
                                int depth = container.GetChildIndex(pieces[x, y].sprite);
                                if (depth > maxDepth)
                                {
                                    maxDepth = depth;
                                    topPiece = pieces[x, y];
                                }
                            }
                        }
                    }

                    if (topPiece != null && Input.GetMouseButtonDown(0))
                    {
                        heldPiece = topPiece;
                    }
                }
                else if (heldPiece != null && Input.GetMouseButton(0))
                {
                    heldPiece.Move(mousePos - lastMousePos);
                }
                else if (heldPiece != null)
                {
                    heldPiece.Drop();
                    heldPiece = null;
                }

                // Update colors
                flashCounter += dt * 60f;
                if (Options.JigsawFlash)
                {
                    foreach (var piece in pieces)
                    {
                        float b;
                        if (heldPiece != null && heldPiece.group.Contains(piece))
                        {
                            b = 0.25f;
                        }
                        else if (heldPiece == null && topPiece != null && topPiece.group.Contains(piece))
                        {
                            b = (Mathf.Cos(flashCounter / 10f) * -0.5f + 0.5f) * Mathf.Lerp(0.25f, 0.05f, (float)topPiece.group.Count / (Width * Height));
                        }
                        else
                        {
                            b = 0;
                        }
                        piece.sprite.color = piece.sprite.color with { b = b };
                    }
                }

                // Update sprite order (pieces first in list are towards front)
                var piecesToSort = piecesList
                    .OrderByDescending(piece => container.GetChildIndex(piece.sprite)) // Make sure most recently grabbed is at top
                    .OrderBy(x => x.group.Contains(heldPiece) ? 0 : 1) // Held pieces above all
                    .ThenBy(x => x.group.Count); // Largest groups at back so as to not obstruct smaller groups
                foreach (var piece in piecesToSort)
                {
                    container.AddChildAtIndex(piece.sprite, 1);
                }

                if (AllConnected)
                {
                    Clear(true);
                }
            }

            // Set for next frame
            lastMousePos = mousePos;

            // Extra keybinds
            if (Input.GetKeyDown(Options.ResetKey) && container != null)
            {
                Clear(false);
            }
            else if (Input.GetKeyDown(Options.ShuffleKey))
            {
                Regenerate();
            }
#if DEBUG
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                CompletionEffect.Yippee();
            }
#endif

            // Update controller
            playStyleController.Update();
        }

        public void Clear(bool doEffect)
        {
            ClearSprites();
            OnClearedPuzzle?.Invoke();
            if (doEffect)
            {
                CompletionEffect.Yippee();
            }
            pieces = null;
            piecesList.Clear();
        }

        public void Destroy()
        {
            ClearSprites();
            playStyleController.Destroy();
        }

        public event Action OnClearedPuzzle;
    }
}
