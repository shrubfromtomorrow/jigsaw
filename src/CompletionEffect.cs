using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jigsaw
{
    public class CompletionEffect
    {
        private static readonly LinkedList<CompletionEffect> activeEffects = [];

        public static void Yippee()
        {
            var (width, height) = Options.GetSize();
            activeEffects.AddLast(new CompletionEffect(width, height));
        }

        internal static void UpdateEffects()
        {
            var node = activeEffects.First;
            while (node != null)
            {
                var next = node.Next;
                node.Value.Update();
                if (node.Value.AllDone)
                {
                    node.Value.Destroy();
                    activeEffects.Remove(node);
                }
                node = next;
            }
        }

        private readonly FContainer container;
        private readonly List<CompletionPiece> pieceList = [];
        private bool AllDone => pieceList.All(x => x.Done);

        private CompletionEffect(int w, int h)
        {
            activeEffects.AddLast(this);

            container = new FContainer();
            Futile.stage.AddChild(container);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    pieceList.Add(new CompletionPiece(container, i, j, w, h));
                }
            }
        }

        private void Update()
        {
            foreach (var piece in pieceList)
            {
                piece.Update();
            }
        }

        private void Destroy()
        {
            foreach (var piece in pieceList)
            {
                piece.Destroy();
            }
            container.RemoveFromContainer();
        }

        private class CompletionPiece
        {
            private readonly int x, y, w, h;
            private readonly FSprite sprite;
            private float t = 0f;

            private float FadeFac => Mathf.Clamp01(t - 0.5f * x / w - 0.5f * y / h);
            private float Fade => Mathf.Cos(FadeFac * Mathf.PI * 2f) * -0.5f + 0.5f;
            public bool Done => FadeFac == 1f;

            public CompletionPiece(FContainer container, int x, int y, int w, int h)
            {
                this.x = x; this.y = y;
                this.w = w; this.h = h;
                var rectSize = new Vector2(Futile.screen.pixelWidth / (float)w, Futile.screen.pixelHeight / (float)h);
                sprite = new FSprite("Futile_White")
                {
                    shader = Shaders.PuzzlePieceOutline,
                    color = new Color(x / 255f, y / 255f, 0f),
                    x = (x + 0.5f) * rectSize.x,
                    y = (y + 0.5f) * rectSize.y,
                    scaleX = rectSize.x / 8f,
                    scaleY = rectSize.y / 8f,
                };
                container.AddChild(sprite);
            }

            public void Update()
            {
                var dt = Time.deltaTime;
                t += dt;

                sprite.color = sprite.color with { b = Fade * 0.5f };
            }

            public void Destroy()
            {
                sprite.RemoveFromContainer();
            }
        }
    }
}
