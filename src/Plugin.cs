using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Jigsaw;

[BepInPlugin("alduris.jigsaw", "Puzzle World", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    private static bool IsInit;
    private static readonly ConditionalWeakTable<RainWorldGame, JigsawContainer> jigsawCWT = new();

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate;
        On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
    }

    private void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        jigsawCWT.Add(self, new JigsawContainer());
    }

    private void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
    {
        orig(self, dt);
        if (jigsawCWT.TryGetValue(self, out var jigsaw))
        {
            jigsaw.Update();
        }
    }

    private void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        if (jigsawCWT.TryGetValue(self, out var jigsaw))
        {
            jigsaw.Destroy();
            jigsawCWT.Remove(self);
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (IsInit) return;
        IsInit = true;

        // Load shaders
        Shaders.LoadShaders();

        // TODO: remix menu
        Logger.LogDebug("Hello world!");
    }
}
