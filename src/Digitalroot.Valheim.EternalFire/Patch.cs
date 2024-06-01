using HarmonyLib;
using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace Digitalroot.Valheim.EternalFire
{
  [UsedImplicitly]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class Patch
  {
    [HarmonyPatch]
    public class PatchFireplaceUpdateFireplace
    {
      [HarmonyPrefix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
      private static void Prefix(ref Fireplace __instance, ref ZNetView ___m_nview)
      {
        if (Main.ConfigCheck(__instance.name)) ___m_nview.GetZDO().Set("fuel", __instance.m_maxFuel);
      }
    }

    #region CookingStation

    [HarmonyPatch]
    public class PatchCookingStationSetFuel
    {
      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.SetFuel))]
      private static void Prefix(ref CookingStation __instance, ref float fuel)
      {
        if (Main.ConfigCheck(__instance.name)) fuel = __instance.m_maxFuel;
      }
    }

    [HarmonyPatch]
    public class PatchCookingStationAwake
    {
      [HarmonyPostfix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.Awake))]
      private static void Postfix(ref CookingStation __instance, ref ZNetView ___m_nview)
      {
        if (!___m_nview.isActiveAndEnabled || Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting()) return;
        if (Main.ConfigCheck(__instance.name)) Main.Refuel(___m_nview);
      }
    }

    #endregion

    #region Smelter

    [HarmonyPatch]
    public class PatchSmelterSetFuel
    {
      [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.SetFuel))]
      private static void Prefix(ref Smelter __instance, ref float fuel)
      {
        if (Main.ConfigCheck(__instance.name)) fuel = __instance.m_maxFuel;
      }
    }

    [HarmonyPatch]
    public class PatchSmelterAwake
    {
      [HarmonyPostfix, HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
      private static void Postfix(ref Smelter __instance, ref ZNetView ___m_nview)
      {
        if (!___m_nview.isActiveAndEnabled || Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting()) return;

        if (Main.ConfigCheck(__instance.name)) Main.Refuel(___m_nview);
      }
    }

    #endregion
  }
}
