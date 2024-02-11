using UnityEngine;
using HMLLibrary;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;

class BetterNametags : Mod
{
    internal static BetterNametags Instance = null;
    private Harmony harmonyInstance;

    public void Start()
    {
        Instance = this;
        harmonyInstance = new Harmony("com.TechGuard.BetterNametags");
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        Log("Mod BetterNametags has been loaded!");
    }

    public void OnModUnload()
    {
        harmonyInstance.UnpatchAll(harmonyInstance.Id);

        Log("Mod BetterNametags has been unloaded!");
    }

    internal static bool NametagShowDistance
    {
        get
        {
            if (ExtraSettingsAPI_Loaded)
            {
                return ExtraSettingsAPI_GetCheckboxState("NametagShowDistance");
            }
            else
            {
                return true;
            }
        }
    }

    internal static float NametagMaxDistance
    {
        get
        {
            if (ExtraSettingsAPI_Loaded)
            {
                return ExtraSettingsAPI_GetSliderValue("NametagMaxDistance");
            }
            else
            {
                return 250;
            }
        }
    }

    internal static float NametagSize
    {
        get
        {
            if (ExtraSettingsAPI_Loaded)
            {
                return ExtraSettingsAPI_GetSliderValue("NametagSize");
            }
            else
            {
                return 0.5f;
            }
        }
    }

    // The following methods are available when 'ExtraSettings' is installed

    [MethodImpl(MethodImplOptions.NoInlining)]
    static float ExtraSettingsAPI_GetSliderValue(string SettingName) => 0;
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ExtraSettingsAPI_GetCheckboxState(string SettingName) => false;

    static bool ExtraSettingsAPI_Loaded = false;
}

[HarmonyPatch(typeof(BillboardObject), "Update")]
class HarmonyPatch_BillboardObject_Update
{
    [HarmonyPrefix]
    internal static bool Prefix(BillboardObject __instance)
    {
        // Find private variables
        var traverse = Traverse.Create(__instance);
        var hideDistanceFromCamera = traverse.Field<float>("hideDistanceFromCamera");
        var bilboardObject = traverse.Field<GameObject>("bilboardObject");
        var targetToLookAt = traverse.Field<Transform>("targetToLookAt");

        if (__instance.transform == null || targetToLookAt.Value == null)
            return true;

        // Override render distance
        hideDistanceFromCamera.Value = BetterNametags.NametagMaxDistance;

        if (bilboardObject.Value == null)
            return true;

        // Make size on screen the same regardless of distance
        float dist = Vector3.Distance(__instance.transform.position, targetToLookAt.Value.position);
        float distScale = 0.2f * BetterNametags.NametagSize;
        bilboardObject.Value.transform.localScale = Vector3.one * dist * distScale;

        // Show distance to target
        var textMesh = bilboardObject.Value.GetComponent<TextMesh>();
        if (textMesh == null)
            return true;

        var newText = textMesh.text;

        var idx = newText.LastIndexOf("<size");
        if (idx > -1)
        {
            newText = newText.Substring(0, idx);
        }

        if (BetterNametags.NametagShowDistance)
        {
            int distance = Mathf.RoundToInt(dist);
            if (distance > 15)
            {
                newText += "<size=\"80%\"> (" + distance + "m)</size>";
                textMesh.richText = true;
            }
        }

        if (textMesh.text != newText)
        {
            textMesh.text = newText;
        }

        return true;
    }
}
