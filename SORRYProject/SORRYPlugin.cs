﻿using BepInEx;
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
using UnityEngine;

namespace SORRY;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class SORRYPlugin : BaseSpaceWarpPlugin
{
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    public static SORRYPlugin Instance { get; set; }


    private bool _isWindowOpen;
    private Rect _windowRect;

    private const string ToolbarOABButtonID = "BTN-SORRY";

    public static string Path { get; private set; }
    public override void OnPreInitialized()
    {
        SORRYPlugin.Path = base.PluginFolderPath;
    }

    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        Harmony.CreateAndPatchAll(typeof(SORRYPlugin).Assembly);
                string partsToSplit = "gridfin, rcsNosecone, Milrin-07, Reiptor-12, cover_5m_0-5-10, ProceduralEngineCover, BE-4, Reiptor-12Vac, GridfinHex, InlineGridfinBase, InlineGridfinBase_2.5m";
                string[] parts = partsToSplit.Split(',').Select(a => a.Trim()).ToArray();

                if (parts is not null && parts.Length > 0)
                    Colors.DeclareParts(MyPluginInfo.PLUGIN_GUID, parts);

        Appbar.RegisterOABAppButton(
            "SORRY",
            ToolbarOABButtonID,
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
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
                GUILayout.Height(500),
                GUILayout.Width(350)
            );
        }
    }
    static List<Module_ProceduralEngineCover> covers = new();
    internal static bool Debug = false;

    private static void FillWindow(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 40));
        if (covers is null || covers.Count == 0)
            GUILayout.Label("Oh... Hi!");
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

    public static void Register(Module_ProceduralEngineCover cover) => covers.Add(cover);
    public static void Unregister(Module_ProceduralEngineCover cover) => covers.Remove(cover);
}
[HarmonyPatch]
internal static class Patcher
{
    [HarmonyPatch]
    internal class DisableThrottleBlendshapeDataCheck
    {
        [HarmonyPatch(typeof(ThrottleBlendshapeData), "OnEnable")]
        internal static bool Prefix(ref ThrottleBlendshapeData __instance)
        {
            __instance.BlendShapeMesh = __instance.GetComponent<SkinnedMeshRenderer>();
            __instance.TriggerUpdateVisuals = (Action<float, float, float, Vector3>)Delegate.Combine(__instance.TriggerUpdateVisuals, new Action<float, float, float, Vector3>(__instance.UpdateVisuals));
            if (__instance.BlendShapeMesh != null)
            {
                System.Random random = new System.Random();
                for (int i = 0; i < __instance.BlendShapeMesh.materials.Length; i++)
                {
                    __instance.BlendShapeMesh.materials[i].SetFloat("_Seed", (float)random.NextDouble());
                }
            }
            return true;
        }
    }
}