using BepInEx;
using HarmonyLib;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace SORRY;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class SORRYPlugin : BaseSpaceWarpPlugin
{
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    public static SORRYPlugin Instance { get; set; }
    public static string Path { get; private set; }

    public override void OnPreInitialized()
    {
        SORRYPlugin.Path = base.PluginFolderPath;
    }

    public override void OnInitialized()
    {
        base.OnInitialized();
        ColorsPatch.DeclareParts(MyPluginInfo.PLUGIN_GUID, new List<string>() { "rcsNosecone", "gridfin" });

        Instance = this;
        
        Harmony.CreateAndPatchAll(typeof(SORRYPlugin).Assembly);
    }

    public override void OnPostInitialized()
    {
        base.OnPostInitialized();
    }
}
