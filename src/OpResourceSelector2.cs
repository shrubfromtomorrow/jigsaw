using UnityEngine;

namespace Jigsaw
{
    public class OpResourceSelector2 : Menu.Remix.MixedUI.OpResourceSelector
    {
        public OpResourceSelector2(ConfigurableBase config, Vector2 pos, float width) : base(config, pos, width) { }
        public OpResourceSelector2(Configurable<string> config, Vector2 pos, float width, SpecialEnum listType) : base(config, pos, width, listType) { }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (_rectList != null && !_rectList.isHidden)
            {
                myContainer.MoveToFront();

                for (int j = 0; j < 9; j++)
                {
                    _rectList.sprites[j].alpha = 1;
                }
            }
        }
    }
}
