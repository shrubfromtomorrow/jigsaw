using System;
using System.IO;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Jigsaw
{
    public class Options : OptionInterface
    {
        public static Options Instance { get; private set; }

        private static Texture2D logoTex;

        private static Configurable<DifficultyPreset> preset;
        private static Configurable<int> width;
        private static Configurable<int> height;
        private static Configurable<bool> jigsawFlash;
        private static Configurable<bool> doArena;
        private static Configurable<KeyCode> resetKey;
        private static Configurable<KeyCode> shuffleKey;
        private static Configurable<PlayStyle> playStyle;
        private static Configurable<float> playStyleChance;

        public static bool JigsawFlash => jigsawFlash.Value;
        public static bool DoArena => doArena.Value;
        public static KeyCode ResetKey => resetKey.Value;
        public static KeyCode ShuffleKey => shuffleKey.Value;
        public static PlayStyle SelectedPlayStyle => playStyle.Value;
        public static float PlayStyleChanceModifier => playStyleChance.Value;

        public Options()
        {
            Instance = this;

            var defaultDifficulty = DifficultyPreset.Small;
            var defaultSize = DifficultyPresetToSize(defaultDifficulty);
            var maxSize = DifficultyPresetToSize(DifficultyPreset.MAX);

            preset ??= config.Bind(nameof(preset), defaultDifficulty);

            width ??= config.Bind(nameof(width), defaultSize.width, new ConfigAcceptableRange<int>(2, maxSize.width));
            height ??= config.Bind(nameof(height), defaultSize.height, new ConfigAcceptableRange<int>(2, maxSize.height));

            jigsawFlash ??= config.Bind(nameof(jigsawFlash), true);
            doArena ??= config.Bind(nameof(doArena), true);

            resetKey ??= config.Bind(nameof(resetKey), KeyCode.F7);
            shuffleKey ??= config.Bind(nameof(shuffleKey), KeyCode.F8);

            playStyle ??= config.Bind(nameof(playStyle), PlayStyle.LetMePuzzle);
            playStyleChance ??= config.Bind(nameof(playStyleChance), 0.5f, new ConfigAcceptableRange<float>(0f, 1f));
        }

        public override void Initialize()
        {
            base.Initialize();

            // Init tab
            var tab = new OpTab(this)
            {
                colorCanvas = MenuColorEffect.rgbDarkGrey
            };
            Tabs = [tab];

            // Load image
            LoadFile("jigsawLogo", ref logoTex);

            // Do position calculations
            const float menuSize = 600f;
            const float menuHalfSize = menuSize / 2;
            const float rowHeight = 36f; // must be at least 30
            const float rowHalfHeight = rowHeight / 2;
            const float rectWidth = 300f;
            const float optionsHeight = rowHeight * 7;
            const float keybindsHeight = rowHeight * 2;
            const float boxMargin = 10f;

            float rectStartX = menuHalfSize - rectWidth / 2;
            float rectEndX = rectStartX + rectWidth - 10f; // 10f margin

            float totalHeight = logoTex.height + boxMargin * 1.5f + optionsHeight + boxMargin + keybindsHeight;
            float totalStartY = menuHalfSize - totalHeight / 2;
            float totalEndY = menuHalfSize + totalHeight / 2;
            float optionsStartY = totalStartY + keybindsHeight + boxMargin;
            float keybindsStartY = totalStartY;
            float titleStartY = totalEndY - logoTex.height;


            // MenuText shader
            var menuTextShader = Custom.rainWorld.Shaders["MenuText"];

            // Init title text
            var titleLabel = new OpImage(new Vector2(menuHalfSize - logoTex.width / 2, titleStartY), "jigsawLogo")
            {
                color = MenuColorEffect.rgbMediumGrey,
            };
            titleLabel.sprite.shader = menuTextShader;
            /*var titleLabel = new OpLabel(new Vector2(menuHalfSize, titleStartY), new Vector2(0f, rowHeight), "P U Z Z L E   W O R L D", FLabelAlignment.Center, true)
            {
                verticalAlignment = OpLabel.LabelVAlignment.Center
            };
            titleLabel.label.shader = menuTextShader;*/

            // Init rects
            var rectOptions = new OpRect(new Vector2(rectStartX, optionsStartY), new Vector2(rectWidth, optionsHeight));
            var rectKeybinds = new OpRect(new Vector2(rectStartX, keybindsStartY), new Vector2(rectWidth, keybindsHeight));

            /*for (int i = 0; i < 4; i++)
            {
                rectOptions.rect.sprites[rectOptions.rect.SideSprite(i)].shader = menuTextShader;
                rectOptions.rect.sprites[rectOptions.rect.CornerSprite(i)].shader = menuTextShader;
                rectKeybinds.rect.sprites[rectKeybinds.rect.SideSprite(i)].shader = menuTextShader;
                rectKeybinds.rect.sprites[rectKeybinds.rect.CornerSprite(i)].shader = menuTextShader;
            }*/

            // Add everything else, plus save some inputs
            OpResourceSelector presetInput, playStyleInput;
            OpUpdown widthInput, heightInput;
            OpLabel sizeLabel, noChanceLabel;
            OpFloatSlider playStyleSlider;
            tab.AddItems([
                titleLabel,
                rectOptions,
                rectKeybinds,


                // Options
                
                // Preset input
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 6), new Vector2(0f, rowHeight), "Preset:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                presetInput = new OpResourceSelector2(preset, new Vector2(rectEndX - 160f, optionsStartY + rowHeight * 6 + rowHalfHeight - 12f), 160f)
                {
                    listHeight = (ushort)Enum.GetValues(typeof(DifficultyPreset)).Length
                },

                // Size inputs
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 5), new Vector2(0f, rowHeight), "Size:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                widthInput = new OpUpdown(width, new Vector2(rectEndX - 140f, optionsStartY + rowHeight * 5 + rowHalfHeight - 15f), 60f),
                new OpLabel(new Vector2(rectEndX - 80f, optionsStartY + rowHeight * 5), new Vector2(20f, rowHeight), "x", FLabelAlignment.Center, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                heightInput = new OpUpdown(height, new Vector2(rectEndX - 60f, optionsStartY + rowHeight * 5 + rowHalfHeight - 15f), 60f),

                // Size label
                sizeLabel = new OpLabel(new Vector2(rectStartX, optionsStartY + rowHeight * 4), new Vector2(rectWidth, 40f), $"{width.Value * height.Value} pieces", FLabelAlignment.Center, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },


                // Play style
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 3), new Vector2(0f, rowHeight), "Play style:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                playStyleInput = new OpResourceSelector2(playStyle, new Vector2(rectEndX - 160f, optionsStartY + rowHeight * 3 + rowHalfHeight - 12f), 160f)
                {
                    description = PlayStyleDescription(playStyle.Value)
                },

                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 2), new Vector2(0f, rowHeight), "Chance modifier:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                playStyleSlider = new OpFloatSlider(playStyleChance, new Vector2(rectEndX - 160f, optionsStartY + rowHeight * 2 + rowHalfHeight - 15f), 160, 2, false)
                {
                    _dNum = 2,
                },
                noChanceLabel = new OpLabel(new Vector2(rectEndX - LabelTest.GetWidth("[does not apply]"), optionsStartY + rowHeight * 2), new Vector2(0f, 40f), "[does not apply]", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },


                // Flashing checkbox
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 1), new Vector2(0f, rowHeight), "Flash on hover:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpCheckBox(jigsawFlash, new Vector2(rectEndX - 24f, optionsStartY + rowHeight * 1 + rowHalfHeight - 12f)),

                // Arena checkbox
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 0), new Vector2(0f, rowHeight), "Arena too:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpCheckBox(doArena, new Vector2(rectEndX - 24f, optionsStartY + rowHeight * 0 + rowHalfHeight - 12f)),


                // Keybinds

                // Reset keybind
                new OpLabel(new Vector2(rectStartX + 10f, keybindsStartY + rowHeight * 1), new Vector2(0f, 40f), "Reset puzzle:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpKeyBinder(resetKey, new Vector2(rectEndX - 60f, keybindsStartY + rowHeight * 1 + rowHalfHeight - 15f), new Vector2(60f, 30f), true),

                // Shuffle keybind
                new OpLabel(new Vector2(rectStartX + 10f, keybindsStartY + rowHeight * 0), new Vector2(0f, 40f), "Regenerate puzzle:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpKeyBinder(shuffleKey, new Vector2(rectEndX - 60f, keybindsStartY + rowHeight * 0 + rowHalfHeight - 15f), new Vector2(60f, 30f), true),
                ]);

            // Update slider because it breaks when greyed out for some reason
            if (PlayStyleUsesRandomChance(playStyle.Value))
            {
                noChanceLabel.Hide();
            }
            else
            {
                playStyleSlider.Hide();
            }

            // Events
            presetInput.OnValueChanged += PresetInput_OnValueChanged;
            widthInput.OnValueChanged += Updown_OnValueChanged;
            heightInput.OnValueChanged += Updown_OnValueChanged;
            playStyleInput.OnValueChanged += PlayStyleInput_OnValueChanged;

            void PresetInput_OnValueChanged(UIconfig self, string value, string oldValue)
            {
                var preset = (DifficultyPreset)Enum.Parse(typeof(DifficultyPreset), presetInput.value);
                if (preset == DifficultyPreset.Custom) return;
                var (widthVal, heightVal) = DifficultyPresetToSize(preset);
                widthInput.valueInt = widthVal;
                heightInput.valueInt = heightVal;
                sizeLabel.text = $"{widthVal * heightVal} pieces";
            }

            void Updown_OnValueChanged(UIconfig self, string value, string oldValue)
            {
                foreach (DifficultyPreset type in Enum.GetValues(typeof(DifficultyPreset)))
                {
                    if (type == DifficultyPreset.Custom) continue;

                    var (width, height) = DifficultyPresetToSize(type);
                    if (width == widthInput.valueInt && height == heightInput.valueInt)
                    {
                        presetInput.value = type.ToString();
                        sizeLabel.text = $"{width * height} pieces";
                        return;
                    }
                }
                presetInput.value = DifficultyPreset.Custom.ToString();
                sizeLabel.text = $"{widthInput.valueInt * heightInput.valueInt} pieces";
            }

            void PlayStyleInput_OnValueChanged(UIconfig self, string value, string oldValue)
            {
                var playStyle = (PlayStyle)Enum.Parse (typeof(PlayStyle), playStyleInput.value);
                playStyleInput.description = PlayStyleDescription(playStyle);
                
                if (PlayStyleUsesRandomChance(playStyle))
                {
                    noChanceLabel.Hide();
                    playStyleSlider.Show();
                }
                else
                {
                    playStyleSlider.Hide();
                    noChanceLabel.Show();
                }
            }
        }

        private void LoadFile(string fileName, ref Texture2D tex)
        {
            if (Futile.atlasManager.GetAtlasWithName(fileName) != null) return;
            string path = AssetManager.ResolveFilePath(Path.Combine("Illustrations", fileName + ".png"));
            tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            AssetManager.SafeWWWLoadTexture(ref tex, "file:///" + path, true, true);
            Futile.atlasManager.LoadAtlasFromTexture(fileName, tex, false);
        }

        public enum DifficultyPreset
        {
            Custom,
            Baby,
            Small,
            Medium,
            Large,
            Gigantic,
            Enormous,
            MAX
        }

        public static (int width, int height) DifficultyPresetToSize(DifficultyPreset preset)
        {
            return preset switch
            {
                DifficultyPreset.Custom => (width.Value, height.Value),
                DifficultyPreset.Baby => (3, 2),       // 6 pieces
                DifficultyPreset.Small => (5, 3),      // 15 pieces
                DifficultyPreset.Medium => (7, 4),     // 28 pieces
                DifficultyPreset.Large => (9, 5),      // 45 pieces (~50)
                DifficultyPreset.Gigantic => (13, 7),  // 91 pieces (~100)
                DifficultyPreset.Enormous => (20, 11), // 220 pieces (~200)
                DifficultyPreset.MAX => (68, 38),      // 2584 pieces (size where each piece's main body is roughly 20x20)
                _ => throw new ArgumentException(nameof(preset)),
            };
        }

        public static (int width, int height) GetSize() => DifficultyPresetToSize(preset.Value);


        public enum PlayStyle
        {
            LetMePuzzle,
            PuzzlePerCycle,
            PuzzlePerRoomPerCycle,
            PuzzlePerRoomPerSession,
            PuzzlePerRoomSometimes,
            PuzzleAtRandom
        }

        public static string PlayStyleDescription(PlayStyle style)
        {
            return style switch
            {
                PlayStyle.LetMePuzzle => "A non-obtrusive button in the top left of the screen, or keybind-activated.",
                PlayStyle.PuzzlePerCycle => "At some point during the cycle, the game will launch a surprise puzzle.",
                PlayStyle.PuzzlePerRoomPerCycle => "A puzzle will appear per room that you have not solved in the current cycle. Leaving an unfinished room and coming back re-randomizes the puzzle.",
                PlayStyle.PuzzlePerRoomPerSession => "A puzzle will appear per room that you have not solved in the current game instance. Leaving an unfinished room and coming back re-randomizes the puzzle.",
                PlayStyle.PuzzlePerRoomSometimes => "A puzzle will sometimes appear in a room on a given cycle. Leaving an unfinished room and coming back re-randomizes the puzzle.",
                PlayStyle.PuzzleAtRandom => "Randomly while there isn't an active puzzle, a puzzle will spawn.",
                _ => throw new NotImplementedException()
            };
        }

        public static bool PlayStyleUsesRandomChance(PlayStyle style) => style == PlayStyle.PuzzleAtRandom || style == PlayStyle.PuzzlePerRoomSometimes;
    }
}
