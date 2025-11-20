using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Jigsaw.PlayStyles
{
    internal class LetMePuzzleController : PlayStyleController
    {
        private bool canPuzzle = true;
        private DyeableRect puzzleRect;
        private FSprite puzzleSprite;
        private Vector2 pos;
        private const float size = 40f;

        public LetMePuzzleController(JigsawContainer jigsaw, RainWorldGame game) : base(jigsaw, game)
        {
            pos = new Vector2(size / 2 + 12f, Futile.screen.pixelHeight - size / 2 - 12f) + new Vector2(0.01f, 0.01f); // centered

            // Underlying rectangle
            puzzleRect = new DyeableRect(Futile.stage, pos - Vector2.one * size / 2, Vector2.one * size, true)
            {
                fillAlpha = 0.3f
            };
            puzzleRect.Update();
            puzzleRect.GrafUpdate(1f);

            // Puzzle piece sprite
            puzzleSprite = new FSprite("Futile_White")
            {
                shader = Shaders.PuzzlePieceColor,
                scale = 40f / 16f,
                x = pos.x,
                y = pos.y,
            };
            Futile.stage.AddChild(puzzleSprite);


            jigsaw.OnClearedPuzzle += Jigsaw_OnClearedPuzzle;
        }

        public override void Update()
        {
            if (canPuzzle)
            {
                var rect = new Rect(pos - Vector2.one * size * 0.5f, Vector2.one * size);
                Vector2 mousePos = Futile.mousePosition;
                bool hovering = rect.Contains(mousePos);

                // Re-add to container if necessary
                if (puzzleSprite.container == null)
                {
                    Futile.stage.AddChild(puzzleRect.container);
                    Futile.stage.AddChild(puzzleSprite);
                }

                // Move sprites to front
                puzzleRect.container.MoveToFront();
                puzzleSprite.MoveToFront();

                // Update jigsaw sprite
                puzzleSprite.color = hovering ? Color.white : MenuColorEffect.rgbMediumGrey;

                // Click behavior
                if (hovering && Input.GetMouseButtonDown(0))
                {
                    puzzleRect.container.RemoveFromContainer();
                    puzzleSprite.RemoveFromContainer();

                    canPuzzle = false;
                    _jigsaw.Regenerate();
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            puzzleSprite.RemoveFromContainer();
            puzzleRect.container.RemoveFromContainer();
        }

        private void Jigsaw_OnClearedPuzzle()
        {
            canPuzzle = true;
        }
    }
}
