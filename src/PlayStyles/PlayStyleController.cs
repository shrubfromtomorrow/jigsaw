using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jigsaw.PlayStyles
{
    public abstract class PlayStyleController(JigsawContainer jigsaw, RainWorldGame game)
    {
        protected readonly JigsawContainer _jigsaw = jigsaw;
        protected readonly RainWorldGame _game = game;

        public abstract void Update();

        public virtual void Destroy() { }

        public static PlayStyleController GetPlayStyleControllerFor(Options.PlayStyle playStyle, JigsawContainer jigsaw, RainWorldGame game)
        {
            return playStyle switch
            {
                Options.PlayStyle.LetMePuzzle => new LetMePuzzleController(jigsaw, game),
                Options.PlayStyle.PuzzlePerCycle => new PuzzlePerCycleController(jigsaw, game),
                Options.PlayStyle.PuzzlePerRoomPerCycle => new PuzzlePerRoomController(jigsaw, game, true, false),
                Options.PlayStyle.PuzzlePerRoomPerSession => new PuzzlePerRoomController(jigsaw, game, false, false),
                Options.PlayStyle.PuzzlePerRoomSometimes => new PuzzlePerRoomController(jigsaw, game, true, true),
                Options.PlayStyle.PuzzleAtRandom => new PuzzleAtRandomController(jigsaw, game),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
