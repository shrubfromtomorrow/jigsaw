using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Jigsaw
{
    public sealed class JigsawContainer
    {
        public JigsawContainer()
        {
            Regenerate(5, 3);
        }

        public JigsawPiece[,] pieces = null;
        private FContainer container = null;
        private FSprite backgroundSprite = null;

        private JigsawPiece heldPiece = null;
        private Vector2 lastMousePos;

        public void Regenerate(int width, int height)
        {
            ClearSprites();

            pieces = new JigsawPiece[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pieces[x, y] = new JigsawPiece(this, x, y);
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
            Futile.instance._cameraImage.texture.filterMode = FilterMode.Point;
            container?.MoveToFront();

            Vector2 mousePos = Futile.mousePosition;

            if (pieces != null)
            {
                // Determine/move held piece
                if (heldPiece == null && Input.GetMouseButtonDown(0))
                {
                    var candidatePieces = new List<JigsawPiece>();
                    for (int x = 0; x < pieces.GetLength(0); x++)
                    {
                        for (int y = 0; y < pieces.GetLength(1); y++)
                        {
                            if (pieces[x, y].MouseIntersecting)
                            {
                                candidatePieces.Add(pieces[x, y]);
                            }
                        }
                    }

                    if (candidatePieces.Count > 0)
                    {
                        heldPiece = candidatePieces.OrderByDescending(piece => container.GetChildIndex(piece.sprite)).First();
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

                // Update sprite order (pieces first in list are towards front)
                List<JigsawPiece> piecesToSort = [.. pieces];
                piecesToSort = [.. piecesToSort.OrderBy(x => x.group.Contains(heldPiece) ? 0 : 1).ThenBy(x => x.group.Count)];
                foreach (var piece in piecesToSort)
                {
                    container.AddChildAtIndex(piece.sprite, 1);
                }
            }

            // Set for next frame
            lastMousePos = mousePos;
        }

        public void Destroy()
        {
            ClearSprites();
        }
    }
}
