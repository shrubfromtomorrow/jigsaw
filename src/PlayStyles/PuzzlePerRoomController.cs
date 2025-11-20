using System.Collections.Generic;
using UnityEngine;

namespace Jigsaw.PlayStyles
{
    internal class PuzzlePerRoomController : PlayStyleController
    {
        private static readonly HashSet<string> globalClearedRooms = [];
        private readonly HashSet<string> currentClearedRooms = [];

        private readonly bool perCycle;
        private readonly bool random;
        private readonly int randomSeed;
        private string lastRoom = null;

        public PuzzlePerRoomController(JigsawContainer jigsaw, RainWorldGame game, bool perCycle, bool random) : base(jigsaw, game)
        {
            this.perCycle = perCycle;
            this.random = random;
            randomSeed = Random.Range(int.MinValue, int.MaxValue);
            jigsaw.OnClearedPuzzle += Jigsaw_OnClearedPuzzle;
        }

        private void Jigsaw_OnClearedPuzzle()
        {
            var roomName = _game.cameras[0].room?.abstractRoom?.name;
            if (roomName != null)
            {
                MarkRoomAsDone(roomName);
            }
        }

        public override void Update()
        {
            var currRoom = _game.cameras[0].room?.abstractRoom?.name;
            if (currRoom != null)
            {
                if (lastRoom != currRoom)
                {
                    if (!DidRoom(currRoom) && (!random || RandomRoll(currRoom, randomSeed)))
                    {
                        _jigsaw.Regenerate();
                    }
                    else
                    {
                        _jigsaw.Clear(false);
                    }
                }
                lastRoom = currRoom;
            }
        }

        private bool DidRoom(string roomName) => perCycle ? currentClearedRooms.Contains(roomName) : globalClearedRooms.Contains(roomName);
        private void MarkRoomAsDone(string roomName)
        {
            globalClearedRooms.Add(roomName);
            if (perCycle) currentClearedRooms.Add(roomName);
        }

        private bool RandomRoll(string roomName, int salt)
        {
            var oldState = Random.state;
            Random.InitState(unchecked(roomName.GetHashCode() + salt));
            bool result = Random.value < Options.PlayStyleChanceModifier;
            Random.state = oldState;
            return result;
        }
    }
}
