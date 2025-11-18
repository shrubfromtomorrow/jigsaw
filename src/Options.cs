using System;
using System.IO;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Jigsaw
{
    internal class Options : OptionInterface
    {
        public static Options Instance { get; private set; }

        private static Texture2D logoTex;

        private static Configurable<DifficultyPreset> preset;
        private static Configurable<int> width;
        private static Configurable<int> height;
        private static Configurable<bool> jigsawFlash;
        private static Configurable<bool> immediate;
        private static Configurable<bool> doArena;
        private static Configurable<KeyCode> resetKey;
        private static Configurable<KeyCode> shuffleKey;

        public static bool JigsawFlash => jigsawFlash.Value;
        public static bool JigsawImmediately => immediate.Value;
        public static bool DoArena => doArena.Value;
        public static KeyCode ResetKey => resetKey.Value;
        public static KeyCode ShuffleKey => shuffleKey.Value;

        public Options()
        {
            Instance = this;

            var defaultDifficulty = DifficultyPreset.Easy;
            var defaultSize = DifficultyPresetToSize(defaultDifficulty);
            var maxSize = DifficultyPresetToSize(DifficultyPreset.MAX);

            preset = config.Bind(nameof(preset), defaultDifficulty);

            width = config.Bind(nameof(width), defaultSize.width, new ConfigAcceptableRange<int>(1, maxSize.width));
            height = config.Bind(nameof(height), defaultSize.height, new ConfigAcceptableRange<int>(1, maxSize.height));

            jigsawFlash = config.Bind(nameof(jigsawFlash), true);
            immediate = config.Bind(nameof(immediate), true);
            doArena = config.Bind(nameof(doArena), true);

            resetKey = config.Bind(nameof(resetKey), KeyCode.F7);
            shuffleKey = config.Bind(nameof(shuffleKey), KeyCode.F8);
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
            const float keybindsHeight = rowHeight * 3;
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
            OpResourceSelector presetInput;
            OpUpdown widthInput, heightInput;
            tab.AddItems([
                titleLabel,
                rectOptions,
                rectKeybinds,


                // Options header label
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 6), new Vector2(0f, rowHeight), "OPTIONS", FLabelAlignment.Left, true)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                
                // Preset input
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 5), new Vector2(0f, rowHeight), "Preset:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                presetInput = new OpResourceSelector2(preset, new Vector2(rectEndX - 160f, optionsStartY + rowHeight * 5 + rowHalfHeight - 12f), 160f)
                {
                    listHeight = (ushort)Enum.GetValues(typeof(DifficultyPreset)).Length
                },

                // Width input
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 4), new Vector2(0f, rowHeight), "Width:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                widthInput = new OpUpdown(width, new Vector2(rectEndX - 60f, optionsStartY + rowHeight * 4 + rowHalfHeight - 15f), 60f),

                // Height input
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 3), new Vector2(0f, rowHeight), "Height:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                heightInput = new OpUpdown(height, new Vector2(rectEndX - 60f, optionsStartY + rowHeight * 3 + rowHalfHeight - 15f), 60f),

                // Flashing checkbox
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 2), new Vector2(0f, rowHeight), "Flash on hover:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpCheckBox(jigsawFlash, new Vector2(rectEndX - 24f, optionsStartY + rowHeight * 2 + rowHalfHeight - 12f)),

                // Immediately checkbox
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 1), new Vector2(0f, rowHeight), "Jigsaw immediately:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpCheckBox(immediate, new Vector2(rectEndX - 24f, optionsStartY + rowHeight * 1 + rowHalfHeight - 12f)),

                // Arena checkbox
                new OpLabel(new Vector2(rectStartX + 10f, optionsStartY + rowHeight * 0), new Vector2(0f, rowHeight), "Arena too:", FLabelAlignment.Left, false)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },
                new OpCheckBox(doArena, new Vector2(rectEndX - 24f, optionsStartY + rowHeight * 0 + rowHalfHeight - 12f)),


                // Keybinds header
                new OpLabel(new Vector2(rectStartX + 10f, keybindsStartY + rowHeight * 2), new Vector2(0f, rowHeight), "KEYBINDS", FLabelAlignment.Left, true)
                {
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                },

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

            // Events
            presetInput.OnValueChanged += PresetInput_OnValueChanged;
            widthInput.OnValueChanged += Updown_OnValueChanged;
            heightInput.OnValueChanged += Updown_OnValueChanged;

            void PresetInput_OnValueChanged(UIconfig self, string value, string oldValue)
            {
                var preset = (DifficultyPreset)Enum.Parse(typeof(DifficultyPreset), presetInput.value);
                if (preset == DifficultyPreset.Custom) return;
                var (widthVal, heightVal) = DifficultyPresetToSize(preset);
                widthInput.valueInt = widthVal;
                heightInput.valueInt = heightVal;
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
                        return;
                    }
                }
                presetInput.value = DifficultyPreset.Custom.ToString();
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
            Easy,
            Medium,
            Hard,
            Gigantic,
            Enormous,
            MAX
        }

        public static (int width, int height) DifficultyPresetToSize(DifficultyPreset preset)
        {
            return preset switch
            {
                DifficultyPreset.Custom => (width.Value, height.Value),
                DifficultyPreset.Baby => (3, 2),
                DifficultyPreset.Easy => (5, 3),
                DifficultyPreset.Medium => (7, 4),
                DifficultyPreset.Hard => (9, 5),
                DifficultyPreset.Gigantic => (13, 7),
                DifficultyPreset.Enormous => (20, 11),
                DifficultyPreset.MAX => (68, 38),
                _ => throw new ArgumentException(nameof(preset)),
            };
        }

        public static (int width, int height) GetSize() => DifficultyPresetToSize(preset.Value);


        // todo: add this as a feature?
        public enum PlayStyle
        {
            LetMePuzzle,
            PuzzlePerCycle,
            PuzzlePerRoom,
            PuzzleAtRandom
        }
    }
}
