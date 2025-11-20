using UnityEngine;

namespace Jigsaw.PlayStyles
{
    internal class PuzzlePerCycleController : PlayStyleController
    {
        private readonly int puzzleTime;
        private bool activated = false;

        public PuzzlePerCycleController(JigsawContainer jigsaw, RainWorldGame game) : base(jigsaw, game)
        {
            puzzleTime = Random.Range(200, (int)(game.world.rainCycle.cycleLength * 0.75)); // 200 gives 5 second grace period
        }

        public override void Update()
        {
            if (!activated && _game.world.rainCycle.timer >= puzzleTime)
            {
                _jigsaw.Regenerate();
                activated = true;
            }
        }
    }
}
