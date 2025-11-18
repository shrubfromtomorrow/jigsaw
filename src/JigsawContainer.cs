using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Jigsaw
{
    public sealed class JigsawContainer
    {
        public JigsawContainer(bool jigsawImmediately)
        {
            if (jigsawImmediately)
            {
                var (width, height) = Options.GetSize();
                Regenerate(width, height);
            }
        }

        public int Width => pieces.GetLength(0);
        public int Height => pieces.GetLength(1);
        private bool AllConnected => pieces[0, 0].group.Count == Width * Height;

        public JigsawPiece[,] pieces = null;
        private readonly List<JigsawPiece> piecesList = [];
        private FContainer container = null;
        private FSprite backgroundSprite = null;

        private JigsawPiece heldPiece = null;
        private Vector2 lastMousePos;
        private int flashCounter = 0;

        public void Regenerate(int width, int height)
        {
            ClearSprites();

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

        public void Update()
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
                flashCounter++;
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
                List<JigsawPiece> piecesToSort = piecesList;
                piecesToSort = [.. piecesToSort.OrderByDescending(piece => container.GetChildIndex(piece.sprite))]; // Make sure most recently grabbed is at top
                piecesToSort = [.. piecesToSort.OrderBy(x => x.group.Contains(heldPiece) ? 0 : 1).ThenBy(x => x.group.Count)];
                foreach (var piece in piecesToSort)
                {
                    container.AddChildAtIndex(piece.sprite, 1);
                }

                if (AllConnected)
                {
                    CompletionEffect.Yippee();
                    ClearSprites();
                }
            }

            // Set for next frame
            lastMousePos = mousePos;

            // Extra keybinds
            if (Input.GetKeyDown(Options.ResetKey) && container != null)
            {
                ClearSprites();
            }
            else if (Input.GetKeyDown(Options.ShuffleKey))
            {
                var (width, height) = Options.GetSize();
                Regenerate(width, height);
            }
        }

        public void Destroy()
        {
            ClearSprites();
        }
    }
}
