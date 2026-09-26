using HarmonyLib;
using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DMF = Digitalroot.Modding.Framework;

namespace Digitalroot.Valheim.EternalFire
{
  [UsedImplicitly]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class Patch
  {
    #region Fireplace

    [HarmonyPatch(typeof(Fireplace))]
    public class PatchFireplace
    {
      [HarmonyPostfix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Awake))]
      private static void PostfixAwake(Fireplace __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;
        ___m_nview.Register(nameof(FireplaceExtensions.RPC_EnableEternal), __instance.RPC_EnableEternal);
        ___m_nview.Register(nameof(FireplaceExtensions.RPC_DisableEternal), __instance.RPC_DisableEternal);
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
      private static void PrefixUpdateFireplace(Fireplace __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;

        if (Main.ConfigCheck(__instance.name)) // Is the Fireplace enabled in the mod's config.
        {
          // Fireplace is not tagged Eternal or m_infiniteFuel is not enabled. 
          if (!__instance.IsEternal() || !__instance.m_infiniteFuel)
          {
            // Tell the owner to tag Fireplace as Eternal and enable m_infiniteFuel.
            ___m_nview.InvokeRPC(nameof(FireplaceExtensions.RPC_EnableEternal));
          }
        }
        else // The Fireplace is disabled in the mod's config.
        {
          // Fireplace is tagged Eternal or m_infiniteFuel is enabled. 
          if (__instance.IsEternal() || __instance.m_infiniteFuel)
          {
            // Tell the owner to untag Fireplace as Eternal and disable m_infiniteFuel.
            ___m_nview.InvokeRPC(nameof(FireplaceExtensions.RPC_DisableEternal));
          }
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
      private static void PostfixGetHoverText(Fireplace __instance, ZNetView ___m_nview, ref string __result)
      {
        if (___m_nview.IsValid() && __instance.IsEternal())
        {
          __result = "<color=#FFFF0088><b>Eternal</b></color>";
        }
      }
    }

    #endregion

    #region CookingStation

    [HarmonyPatch(typeof(CookingStation))]
    public class PatchCookingStation
    {
      [HarmonyPostfix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.Awake))]
      private static void PostfixAwake(CookingStation __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;
        ___m_nview.Register(nameof(CookingStationExtensions.RPC_EnableEternal), __instance.RPC_EnableEternal);
        ___m_nview.Register(nameof(CookingStationExtensions.RPC_DisableEternal), __instance.RPC_DisableEternal);
      }

      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.Update))]
      private static void PrefixUpdate(CookingStation __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;
      }

      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateCooking))]
      private static void PrefixUpdateCooking(CookingStation __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;

        // if mod is enabled
        if (Main.ConfigCheck(__instance.name)) // Is the CookingStation enabled in the mod's config.
        {
          // If mod enabled and CookingStation is not Eternal. Enabled Eternal
          if (!__instance.IsEternal())
          {
            // Tell the owner to tag the CookingStation as Eternal
            ___m_nview.InvokeRPC(nameof(CookingStationExtensions.RPC_EnableEternal));
          }
        }
        else // The CookingStation is disabled in the mod's config.
        {
          // CookingStation is Eternal. Disable Eternal
          if (__instance.IsEternal())
          {
            // Tell the owner to untag the CookingStation as Eternal
            ___m_nview.InvokeRPC(nameof(CookingStationExtensions.RPC_DisableEternal));
          }
        }

        if (__instance.IsEternal() && __instance.GetFuel() == 0f)
        {
          // Tell the owner to add fuel to the CookingStation
          ___m_nview.InvokeRPC(nameof(CookingStation.RPC_AddFuel));
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnHoverFuelSwitch))]
      private static void PostfixOnHoverFuelSwitch(CookingStation __instance, ZNetView ___m_nview, ref string __result)
      {
        if (___m_nview.IsValid() && Main.ConfigCheck(__instance.name))
        {
          __result = "<color=#FFFF0088><b>Eternal</b></color>";
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnAddFuelSwitch))]
      private static bool PostfixOnAddFuelSwitch(CookingStation __instance, ZNetView ___m_nview, ref bool __result)
      {
        if (___m_nview.IsValid() && Main.ConfigCheck(__instance.name))
        {
          __result = false;
          return false;
        }

        return true;
      }
    }

    #endregion

    #region Smelter

    [HarmonyPatch(typeof(Smelter))]
    public class PatchSmelter
    {
      [HarmonyPostfix, HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
      private static void PostfixAwake(Smelter __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;
        ___m_nview.Register(nameof(SmelterExtensions.RPC_EnableEternal), __instance.RPC_EnableEternal);
        ___m_nview.Register(nameof(SmelterExtensions.RPC_DisableEternal), __instance.RPC_DisableEternal);
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.UpdateSmelter))]
      private static void PrefixUpdateSmelter(Smelter __instance, ZNetView ___m_nview)
      {
        if (___m_nview == null || !___m_nview.IsValid()) return;

        // if mod is enabled
        if (Main.ConfigCheck(__instance.name)) // Is the Smelter enabled in the mod's config.
        {
          // If mod enabled and Smelter is not Eternal. Enabled Eternal
          if (!__instance.IsEternal())
          {
            // Tell the owner to tag the Smelter as Eternal
            ___m_nview.InvokeRPC(nameof(SmelterExtensions.RPC_EnableEternal));
          }
        }
        else // The Smelter is disabled in the mod's config.
        {
          // Smelter is Eternal. Disable Eternal
          if (__instance.IsEternal())
          {
            // Tell the owner to untag the Smelter as Eternal
            ___m_nview.InvokeRPC(nameof(SmelterExtensions.RPC_DisableEternal));
          }
        }

        if (__instance.IsEternal() && __instance.GetFuel() == 0f)
        {
          // Tell the owner to add fuel to the Smelter
          ___m_nview.InvokeRPC(nameof(Smelter.RPC_AddFuel));
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddFuel))]
      private static void PostfixOnHoverAddFuel(Smelter __instance, ZNetView ___m_nview, ref string __result)
      {
        if (___m_nview.IsValid() && Main.ConfigCheck(__instance.name))
        {
          __result = "<color=#FFFF0088><b>Eternal</b></color>";
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddFuel))]
      private static bool PostfixOnAddFuel(Smelter __instance, ZNetView ___m_nview, ref bool __result)
      {
        if (___m_nview.IsValid() && Main.ConfigCheck(__instance.name))
        {
          __result = false;
          return false;
        }

        return true;
      }
    }

    #endregion
  }

  [UsedImplicitly]
  public static class FireplaceExtensions
  {
    /// <summary>
    /// Is the current Fireplace Eternal?
    /// </summary>
    /// <param name="fireplace"></param>
    /// <returns>True if the current Fireplace is Eternal</returns>
    [UsedImplicitly]
    public static bool IsEternal(this Fireplace fireplace)
    {
      if (!fireplace.m_nview.IsValid()) return false;
      return fireplace.m_nview.GetZDO().GetBool(Main.Guid.GetStableHashCode());
    }

    /// <summary>
    /// RPC Enable Eternal for Fireplace
    /// </summary>
    /// <param name="fireplace"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this Fireplace fireplace, long sender)
    {
      if (fireplace.m_nview == null
          || !fireplace.m_nview.IsValid()
          || !fireplace.m_nview.IsOwner()
          || fireplace.IsEternal()
          || !Main.ConfigCheck(fireplace.name))
      {
        return;
      }

      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      fireplace.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), true);
      fireplace.m_infiniteFuel = true;
    }

    /// <summary>
    /// RPC Disable Eternal for Fireplace
    /// </summary>
    /// <param name="fireplace"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this Fireplace fireplace, long sender)
    {
      if (fireplace.m_nview == null
          || !fireplace.m_nview.IsValid()
          || !fireplace.m_nview.IsOwner()
          || !fireplace.IsEternal()
          || Main.ConfigCheck(fireplace.name))
      {
        return;
      }

      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      fireplace.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), false);
      fireplace.m_infiniteFuel = false;
    }
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
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this CookingStation cookingStation, long sender)
    {
      if (cookingStation.m_nview == null
          || !cookingStation.m_nview.IsValid()
          || !cookingStation.m_nview.IsOwner()
          || cookingStation.IsEternal()
          || !Main.ConfigCheck(cookingStation.name))
      {
        return;
      }

      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      cookingStation.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), true);
    }

    /// <summary>
    /// Disable Eternal for CookingStation
    /// </summary>
    /// <param name="cookingStation"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this CookingStation cookingStation, long sender)
    {
      if (cookingStation.m_nview == null
          || !cookingStation.m_nview.IsValid()
          || !cookingStation.m_nview.IsOwner()
          || !cookingStation.IsEternal()
          || Main.ConfigCheck(cookingStation.name))
      {
        return;
      }

      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      cookingStation.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), false);
      if (cookingStation.GetFuel() <= 1f)
      {
        cookingStation.SetFuel(0f);
      }
    }
  }

  [UsedImplicitly]
  public static class SmelterExtensions
  {
    /// <summary>
    /// Is the current Smelter Eternal?
    /// </summary>
    /// <param name="smelter"></param>
    /// <returns>True if the current Smelter is Eternal</returns>
    [UsedImplicitly]
    public static bool IsEternal(this Smelter smelter)
    {
      if (!smelter.m_nview.IsValid()) return false;
      return smelter.m_nview.GetZDO().GetBool(Main.Guid.GetStableHashCode());
    }

    /// <summary>
    /// Enable Eternal for Smelter
    /// </summary>
    /// <param name="smelter"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this Smelter smelter, long sender)
    {
      if (smelter.m_nview == null
          || !smelter.m_nview.IsValid()
          || !smelter.m_nview.IsOwner()
          || smelter.IsEternal()
          || !Main.ConfigCheck(smelter.name))
      {
        return;
      }

      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      smelter.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), true);
    }

    /// <summary>
    /// Disable Eternal for Smelter
    /// </summary>
    /// <param name="smelter"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this Smelter smelter, long sender)
    {
      if (smelter.m_nview == null
          || !smelter.m_nview.IsValid()
          || !smelter.m_nview.IsOwner()
          || !smelter.IsEternal()
          || Main.ConfigCheck(smelter.name))
      {
        return;
      }

      DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      smelter.m_nview.GetZDO().Set(Main.Guid.GetStableHashCode(), false);
      if (smelter.GetFuel() <= 1f)
      {
        smelter.SetFuel(0f);
      }
    }
  }
}
