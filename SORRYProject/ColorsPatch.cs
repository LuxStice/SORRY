using HarmonyLib;
using KSP.Modules;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace SORRY;

[HarmonyPatch(typeof(Module_Color),
    nameof(Module_Color.OnInitialize))]
class ColorsPatch
{
    const string DiffuseSuffix = "_d.png";
    const string MetallicSuffix = "_m.png";
    const string BumpSuffix = "_n.png";
    const string AmbientOcclusionSuffix = "_ao.png";
    const string EmissionSuffix = "_e.png";
    const string PaintMapSuffix = "_pm.png";

    public static void Postfix(Module_Color __instance)
    {
        Shader toSet = Shader.Find("KSP2/Scenery/Standard (Opaque)");
        Shader toSet2 = Shader.Find("KSP2/Scenery/Standard (Transparent)");
        Shader toReplace = Shader.Find("Standard");
        string partName = "(undefined)";

        if (__instance.OABPart is not null)
            partName = __instance.OABPart.PartName;
        else
            partName = __instance.part.Name;

        if (partName.Length >= 3)
        {
            if (partName.EndsWith("XS")
                || partName.EndsWith("XL"))
                partName = partName.Remove(partName.Length - 2, 2);

            else if (partName.EndsWith("S") || partName.EndsWith("M")
                || partName.EndsWith("L"))
                partName = partName.Remove(partName.Length - 1);
        }
        

        //caching properties ids for efficiency
        int diffuseTexId =          Shader.PropertyToID("_MainTex");
        int mettalicTexId =         Shader.PropertyToID("_MetallicGlossMap");
        int bumpTexId =             Shader.PropertyToID("_BumpMap");
        int ambientOcclusionTexId = Shader.PropertyToID("_OcclusionMap");
        int emissionTexId =         Shader.PropertyToID("_EmissionMap");
        int paintMapTexId =         Shader.PropertyToID("_PaintMaskGlossMap");

        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer.material.shader.name != toReplace.name)
                continue;

            SORRYPlugin.Instance.ModLogger.LogInfo($"Checking part named {partName}");

            Material mat = new Material(toSet);

            bool flag = false; //important maps. Diffuse ??maybe PaintMap
            bool flag2 = false;

            if (AssetManager.TryGetAsset($"{MyPluginInfo.PLUGIN_GUID}/images/{partName}/{partName}{DiffuseSuffix}", out Texture2D dTex))
            {
                mat.SetTexture(diffuseTexId, dTex);
            }
            else
                continue;

            if (AssetManager.TryGetAsset($"{MyPluginInfo.PLUGIN_GUID}/images/{partName}/{partName}{PaintMapSuffix}", out Texture2D pmTex))
            {
                mat.SetTexture(paintMapTexId, pmTex);
            }
            else
                continue;

            if (AssetManager.TryGetAsset($"{MyPluginInfo.PLUGIN_GUID}/images/{partName}/{partName}{MetallicSuffix}",            out Texture2D mTex))
            {
                mat.SetTexture(mettalicTexId, mTex);
            }
            else
                flag2 = true;

            if (AssetManager.TryGetAsset($"{MyPluginInfo.PLUGIN_GUID}/images/{partName}/{partName}{BumpSuffix}",                out Texture2D bTex))
            {
                mat.SetTexture(bumpTexId, bTex);
            }
            else
                flag2 = true;

            if (AssetManager.TryGetAsset($"{MyPluginInfo.PLUGIN_GUID}/images/{partName}/{partName}{AmbientOcclusionSuffix}",    out Texture2D aoTex))
            {
                mat.SetTexture(ambientOcclusionTexId, aoTex);
            }
            else
                flag2 = true;

            if (AssetManager.TryGetAsset($"{MyPluginInfo.PLUGIN_GUID}/images/{partName}/{partName}{EmissionSuffix}",            out Texture2D eTex))
            {
                mat.SetTexture(emissionTexId, eTex);
            }
            else
                flag2 = true;


            if (flag)
                continue;

            renderer.material = mat;

            if (renderer.material.shader.name != toSet.name)
                renderer.SetMaterial(mat); //Sometimes the material Set doesn't work, this seems to be more reliable.

        }
        __instance.SomeColorUpdated();
    }
}
