using HarmonyLib;
using JetBrains.Annotations;
using DMF = Digitalroot.Modding.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Digitalroot.Valheim.EternalFire
{
  [UsedImplicitly]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class Patch
  {
    [HarmonyPatch(typeof(Fireplace))]
    public class PatchFireplaceUpdateFireplace
    {
      [HarmonyPrefix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
      private static void Prefix(ref Fireplace __instance, ref ZNetView ___m_nview)
      {
        __instance.m_infiniteFuel = Main.ConfigCheck(__instance.name);
      }
    }

    #region CookingStation
    
    [HarmonyPatch(typeof(CookingStation))]
    public class PatchCookingStationSetFuel
    {
      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateCooking))]
      private static void Prefix(ref CookingStation __instance)
      {
        // if CookingStation is Eternal and out of fuel. Add fuel.
        if (__instance.IsEternal())
        {
          if (__instance.GetFuel() == 0f)
          {
            __instance.SetFuel(1f);
          }
        }

        // If mod enabled and CookingStation is not Eternal. Enabled Eternal
        if (Main.ConfigCheck(__instance.name) && !__instance.IsEternal())
        {
            __instance.EnableEternal();
        }

        // If mod disabled and CookingStation is Eternal. Disable Eternal
        if (!Main.ConfigCheck(__instance.name) && __instance.IsEternal())
        {
          __instance.DisableEternal();
          if (__instance.GetFuel() <= 1f)
          {
            __instance.SetFuel(0f);
          }
        }
      }
    }

    
    // [HarmonyPatch]
    // public class PatchCookingStationSetFuel
    // {
    //   [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.SetFuel))]
    //   private static void Prefix(ref CookingStation __instance, ref float fuel)
    //   {
    //     if (Main.ConfigCheck(__instance.name))
    //     {
    //       fuel = __instance.m_maxFuel;
    //     }
    //   }
    // }

    // [HarmonyPatch]
    // public class PatchCookingStationAwake
    // {
    //   [HarmonyPostfix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.Awake))]
    //   private static void Postfix(ref CookingStation __instance, ref ZNetView ___m_nview)
    //   {
    //     if (!___m_nview.isActiveAndEnabled || Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting())
    //     {
    //       return;
    //     }
    //
    //     if (Main.ConfigCheck(__instance.name))
    //     {
    //       Main.Refuel(___m_nview);
    //     }
    //   }
    // }

    #endregion

    #region Smelter

    // [HarmonyPatch]
    // public class PatchSmelterSetFuel
    // {
    //   [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.SetFuel))]
    //   private static void Prefix(ref Smelter __instance, ref float fuel)
    //   {
    //     if (Main.ConfigCheck(__instance.name))
    //     {
    //       fuel = __instance.m_maxFuel;
    //     }
    //   }
    // }
    //
    // [HarmonyPatch]
    // public class PatchSmelterAwake
    // {
    //   [HarmonyPostfix, HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
    //   private static void Postfix(ref Smelter __instance, ref ZNetView ___m_nview)
    //   {
    //     if (!___m_nview.isActiveAndEnabled || Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting())
    //     {
    //       return;
    //     }
    //
    //     if (Main.ConfigCheck(__instance.name))
    //     {
    //       Main.Refuel(___m_nview);
    //     }
    //   }
    // }

    #endregion
  }

  [UsedImplicitly]
  public static class CookingStationExtensions
  {
    /// <summary>
    /// Is the current CookingStation Eternal?
    /// </summary>
    /// <param name="cookingStation"></param>
    /// <returns>True if the current CookingStation is Eternal</returns>
    [UsedImplicitly]
    public static bool IsEternal(this CookingStation cookingStation)
    {
      if (!cookingStation.m_nview.IsValid()) return false;
      return cookingStation.m_nview.GetZDO().GetBool(Main.Guid.GetStableHashCode());
    }

    /// <summary>
    /// Enable Eternal for CookingStation
    /// </summary>
    /// <param name="cookingStation"></param>
    [UsedImplicitly]
    public static void EnableEternal(this CookingStation cookingStation)
    {
      if (!cookingStation.m_nview.IsValid()) return;
      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      // cookingStation.m_useFuel = false;
      cookingStation.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), true);
    }

    /// <summary>
    /// Disable Eternal for CookingStation
    /// </summary>
    /// <param name="cookingStation"></param>
    [UsedImplicitly]
    public static void DisableEternal(this CookingStation cookingStation)
    {
      if (!cookingStation.m_nview.IsValid()) return;
      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      // cookingStation.m_useFuel = true;
      cookingStation.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), false);
    }

    /// <summary>
    /// Toggle Eternal for CookingStation
    /// </summary>
    /// <param name="cookingStation"></param>
    [UsedImplicitly]
    public static void ToggleEternal(this CookingStation cookingStation)
    {
      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      if (cookingStation.IsEternal())
      {
        cookingStation.DisableEternal();
      }
      else
      {
        cookingStation.EnableEternal();
      }
    }
  }
}
