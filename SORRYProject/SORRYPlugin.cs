using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Game;
using KSP.Messages;
using KSP.Messages.PropertyWatchers;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using KSP.UI.Binding;
using KSP.VFX;
using SORRY.Modules;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Parts;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SORRY;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class SORRYPlugin : BaseSpaceWarpPlugin
{
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;


    private const string ToolbarOABButtonID = "BTN-SORRY";


    private bool _isWindowOpen;
    private Rect _windowRect;

    public static SORRYPlugin Instance { get; set; }
    public static string Path { get; private set; }
    public override void OnPreInitialized()
    {
        Path = base.PluginFolderPath;
    }

    public override void OnInitialized()
    {
        base.OnInitialized();
        Instance = this;
        //SDebug.ShowDebug = true;

        Harmony.CreateAndPatchAll(typeof(SORRYPlugin).Assembly);

        byte[] bytes = File.ReadAllBytes(PluginFolderPath + @"/assets/soundbanks/Engines.bnk");
        AkSoundEngine.LoadBankMemoryView(GCHandle.Alloc(bytes, GCHandleType.Pinned).AddrOfPinnedObject(), (uint)bytes.Length, out uint bankId);

        Appbar.RegisterOABAppButton(
            "Procedural Engine Covers",
            ToolbarOABButtonID,
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/PCE_Icon.png"),
            isOpen =>
            {
                _isWindowOpen = isOpen;
                GameObject.Find(ToolbarOABButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );

        Game.Messages.Subscribe<GameStateChangedMessage>(UpdateUIState);
    }

    public void OpenWindow(bool active)
    {
        _isWindowOpen = active;
    }

    void OnDestroy()
    {
        Game.Messages.Unsubscribe<GameStateChangedMessage>(UpdateUIState);
    }

    private void UpdateUIState(MessageCenterMessage obj)
    {
        GameStateChangedMessage message = obj as GameStateChangedMessage;

        if (message.CurrentState != GameState.VehicleAssemblyBuilder)
        {
            _isWindowOpen = false;
        }
    }

    public override void OnPostInitialized()
    {
        base.OnPostInitialized();
    }

    private void OnGUI()
    {
        // Set the UI
        GUI.skin = Skins.ConsoleSkin;

        if (_isWindowOpen)
        {
            _windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                _windowRect,
                FillWindow,
                "SORRY",
                GUILayout.Height(200),
                GUILayout.Width(350)
            );
        }
    }
    static List<Module_ProceduralEngineCover> covers = new();
    internal static bool Debug = false;

    private static void FillWindow(int windowID)
    {
        if (GUI.Button(new Rect(Instance._windowRect.width - 18, 2, 16, 16), "x"))
        {
            Instance._isWindowOpen = false;
            GUIUtility.ExitGUI();
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 40));
        if (covers is null || covers.Count == 0)
            GUILayout.Label("Oh... Hi! add a procedural engine cover to start!");
        int i = 0; 
        foreach (var cover in covers)
        {
            i++;
            if (GUILayout.Button($"{cover.OABPart.PartName} ({i})"))
            {
                cover.ShowWindow(!cover._WindowOpen);
            }
            if (cover._WindowOpen)
                cover.DrawGUI();
        }
    }

    public ManualLogSource ModLogger => Logger;

    public static void Register(Module_ProceduralEngineCover cover) => covers.Add(cover);
    public static void Unregister(Module_ProceduralEngineCover cover) => covers.Remove(cover);
}