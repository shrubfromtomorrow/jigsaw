using UnityEngine;

namespace Jigsaw.PlayStyles
{
    internal class PuzzleAtRandomController(JigsawContainer jigsaw, RainWorldGame game) : PlayStyleController(jigsaw, game)
    {
        private float safetyTimer = 5f;

        public override void Update()
        {
            if (_jigsaw.AllConnected)
            {
                if (safetyTimer <= 0f && Random.value < 0.05f * Time.deltaTime * Options.PlayStyleChanceModifier)
                {
                    _jigsaw.Regenerate();
                    safetyTimer = 5f;
                }
                else
                {
                    safetyTimer -= Time.deltaTime;
                }
            }
        }
    }
}
