using Digitalroot.Modding.Framework.Logging;
using Digitalroot.Modding.Framework.Names.Vanilla;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

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
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid()) return;
          ___m_nview.Register($"{Main.Guid}_{nameof(FireplaceExtensions.RPC_EnableEternal)}", __instance.RPC_EnableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(FireplaceExtensions.RPC_DisableEternal)}", __instance.RPC_DisableEternal);
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [Init.IsEternal: {__instance.IsEternal()}]");
          // __instance.SetIsEternal(__instance.IsEternal());
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
      private static void PrefixUpdateFireplace(Fireplace __instance, ZNetView ___m_nview)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return;

          if (!___m_nview.HasOwner())
          {
            ___m_nview.ClaimOwnership();
          }

          var eternal = __instance.GetIsEternal();
          __instance.m_infiniteFuel = eternal;

          if (Main.ConfigCheck(__instance.name)) // Is the Fireplace enabled in the mod's config.
          {
            // Fireplace is not tagged Eternal
            if (!eternal)
            {
              if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [InvokeRPC.RPC_EnableEternal]");
              // Tell the owner to tag Fireplace as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(FireplaceExtensions.RPC_EnableEternal)}");
            }
          }
          else // The Fireplace is disabled in the mod's config.
          {
            // Fireplace is tagged Eternal 
            if (eternal)
            {
              if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [InvokeRPC.RPC_DisableEternal]");
              // Tell the owner to untag Fireplace as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(FireplaceExtensions.RPC_DisableEternal)}");
            }
          }

          if (__instance.IsEternal() && __instance.GetFuel() <= __instance.m_maxFuel - 1f)
          {
            if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [Adding Fuel to {__instance.name}]");
            // Tell the owner to add fuel to the Fireplace
            ___m_nview.InvokeRPC(nameof(Fireplace.RPC_AddFuelAmount), __instance.m_maxFuel - __instance.GetFuel());
            #if DEBUG
            __instance.TriggerLocator();
            #endif
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
      private static void PostfixGetHoverText(Fireplace __instance, ZNetView ___m_nview, ref string __result)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return;

          // DMF.Logging.Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [{__instance.name}]");
          if (__instance.IsEternal() && __instance.m_infiniteFuel)
          {
            __result = __instance.m_name;
            if (__instance.name.Replace("(Clone)", string.Empty) == PrefabNames.CandleResin)
            {
              __result += "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
            }
            else if (__instance.m_canRefill)
            {
              __result += "\n[<color=yellow><b>$KEY_HotbarUse</b></color>] $piece_useitem";
            }

            __result += $"\n<color={Main.TextColor.Value}><b>$mod_eternal_fire_hover_text</b></color>";

            if (Main.DisplayDebugInfo.Value)
            {
              __result += $"\n( $piece_fire_fuel {Mathf.Ceil(___m_nview.GetZDO().GetFloat(ZDOVars.s_fuel))}/{__instance.m_maxFuel} )";
            }

            __result = Localization.instance.Localize(__result);
          }

          if (Main.DisplayDebugInfo.Value)
          {
            __result += $"\nIsEternalZdo: {__instance.GetIsEternal()}";
            __result += $"\nOriginalFuelLevelZdo: {__instance.GetOriginalFuelLevel()}";
            __result += $"\nIsVPCInstalled: {Main.IsVpcInstalled}";
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
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
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid()) return;
          ___m_nview.Register($"{Main.Guid}_{nameof(CookingStationExtensions.RPC_EnableEternal)}", __instance.RPC_EnableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(CookingStationExtensions.RPC_DisableEternal)}", __instance.RPC_DisableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(CookingStationExtensions.RPC_SetFuelLevelMax)}", __instance.RPC_SetFuelLevelMax);
          // __instance.SetIsEternal(__instance.IsEternal());
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateCooking))]
      private static void PrefixUpdateCooking(CookingStation __instance, ZNetView ___m_nview)
      {
        try
        {
          // Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid() || __instance.m_fuelItem == null || !__instance.m_useFuel) return;

          if (!___m_nview.HasOwner())
          {
            ___m_nview.ClaimOwnership();
          }

          if (Main.ConfigCheck(__instance.name)) // Is the CookingStation enabled in the mod's config.
          {
            // CookingStation is not tagged Eternal.
            if (!__instance.IsEternal())
            {
              // Tell the owner to tag the CookingStation as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(CookingStationExtensions.RPC_EnableEternal)}");
            }
          }
          else // The CookingStation is disabled in the mod's config.
          {
            // CookingStation is tagged Eternal.
            if (__instance.IsEternal())
            {
              // Tell the owner to untag the CookingStation as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(CookingStationExtensions.RPC_DisableEternal)}");
            }
          }

          if (__instance.IsEternal() && __instance.GetFuel() <= __instance.m_maxFuel - 1f)
          {
            if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [RPC_SetFuelLevelMax for {__instance.name}]");

            // Tell the owner to add fuel to the CookingStation
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Main.IsVpcInstalled)
            {
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(CookingStationExtensions.RPC_SetFuelLevelMax)}");
            }
            else
            {
              ___m_nview.InvokeRPC(nameof(CookingStation.RPC_AddFuel));
            }
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnHoverFuelSwitch))]
      private static void PostfixOnHoverFuelSwitch(CookingStation __instance, ZNetView ___m_nview, ref string __result)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return;

          // Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (__instance.IsEternal())
          {
            __result = $"<color={Main.TextColor.Value}><b>$mod_eternal_fire_hover_text</b></color>";
            if (Main.DisplayDebugInfo.Value)
            {
              __result += $"\n( $piece_fire_fuel {Mathf.Ceil(__instance.GetFuel())}/{__instance.m_maxFuel} )";
            }

            __result = Localization.instance.Localize(__result);
          }

          if (Main.DisplayDebugInfo.Value)
          {
            __result += $"\nIsEternalZdo: {__instance.GetIsEternal()}";
            __result += $"\nOriginalFuelLevelZdo: {__instance.GetOriginalFuelLevel()}";
            __result += $"\nIsVPCInstalled: {Main.IsVpcInstalled}";
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnAddFuelSwitch))]
      private static bool PrefixOnAddFuelSwitch(CookingStation __instance, ZNetView ___m_nview, ref bool __result)
      {
        try
        {
          // Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview.IsValid() && Main.ConfigCheck(__instance.name) && __instance.IsEternal())
          {
            __result = false;
            return false;
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }

        return true;
      }
    }

    [HarmonyPatch(typeof(CookingStation))]
    public static class PatchCookingStation_DropAllItems_DropPatch
    {
      [UsedImplicitly]
      private static MethodBase TargetMethod()
      {
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          return AccessTools.Method(typeof(CookingStation), "<DropAllItems>g__drop|1_0"); // fragile, validate after any update to base game.
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }

        return null;
      }

      [HarmonyPrefix, UsedImplicitly] // Target Method is defined above.
      private static bool Prefix(CookingStation __instance, ZNetView ___m_nview, ItemDrop item, bool cheated)
      {
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid()) return true; // If not valid, then skip this patch but continue method chain.

          if (__instance.IsEternal())
          {
            // Only drop original fuel
            for (var i = __instance.m_nview.GetZDO().GetFloat($"{Main.Guid}_currentFuelLevel".GetStableHashCode()); i >= 1f; i--)
            {
              CallOriginal(__instance, item, cheated);
            }

            __instance.SetOriginalFuelLevel(0f); // Reset
            return false;                        // If eternal, do not drop fuel when OnDestroyed is called.
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }

        return true;
      }

      [HarmonyReversePatch]
      public static void CallOriginal(CookingStation instance, ItemDrop item, bool cheated)
      {
        // Harmony replaces this body with the original method's IL at patch time.
        throw new NotImplementedException("HarmonyReversePatch");
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
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid()) return;
          ___m_nview.Register($"{Main.Guid}_{nameof(SmelterExtensions.RPC_EnableEternal)}", __instance.RPC_EnableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(SmelterExtensions.RPC_DisableEternal)}", __instance.RPC_DisableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(SmelterExtensions.RPC_SetFuelLevelMax)}", __instance.RPC_SetFuelLevelMax);
          // __instance.SetIsEternal(__instance.IsEternal());
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.UpdateSmelter))]
      private static void PrefixUpdateSmelter(Smelter __instance, ZNetView ___m_nview)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid() || __instance.m_fuelItem == null) return;

          if (!___m_nview.HasOwner())
          {
            ___m_nview.ClaimOwnership();
          }

          if (Main.ConfigCheck(__instance.name)) // Is the Smelter enabled in the mod's config.
          {
            // Smelter is not tagged Eternal.
            if (!__instance.IsEternal())
            {
              // Tell the owner to tag the Smelter as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(SmelterExtensions.RPC_EnableEternal)}");
            }
          }
          else // The Smelter is disabled in the mod's config.
          {
            // Smelter is tagged Eternal.
            if (__instance.IsEternal())
            {
              // Tell the owner to untag the Smelter as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(SmelterExtensions.RPC_DisableEternal)}");
            }
          }

          if (__instance.IsEternal() && __instance.GetFuel() <= __instance.m_maxFuel - 1f)
          {
            if (Main.Instance.EnableTrace)
            {
              Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [Adding Fuel for {__instance.name}]");
              Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [__instance.GetFuel() : {__instance.GetFuel()}]");
            }

            // Tell the owner to add fuel to the Smelter
            if (Main.IsVpcInstalled)
            {
              if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [RPC_SetFuelLevelMax for {__instance.name}]");
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(SmelterExtensions.RPC_SetFuelLevelMax)}");
            }
            else
            {
              if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [RPC_AddFuel for {__instance.name}]");
              ___m_nview.InvokeRPC(nameof(Smelter.RPC_AddFuel));
            }
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddFuel))]
      private static void PostfixOnHoverAddFuel(Smelter __instance, ZNetView ___m_nview, ref string __result)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return;

          if (__instance.IsEternal())
          {
            __result = $"<color={Main.TextColor.Value}><b>$mod_eternal_fire_hover_text</b></color>";
            if (Main.DisplayDebugInfo.Value)
            {
              __result += $"\n( $piece_fire_fuel {Mathf.Ceil(__instance.GetFuel())}/{__instance.m_maxFuel} )";
            }

            __result = Localization.instance.Localize(__result);
          }

          if (Main.DisplayDebugInfo.Value)
          {
            __result += $"\nIsEternalZdo: {__instance.GetIsEternal()}";
            __result += $"\nOriginalFuelLevelZdo: {__instance.GetOriginalFuelLevel()}";
            __result += $"\nIsVPCInstalled: {Main.IsVpcInstalled}";
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddFuel))]
      private static bool PrefixOnAddFuel(Smelter __instance, ZNetView ___m_nview, ref bool __result)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return true;
          if (__instance.IsEternal())
          {
            __result = false;
            return false;
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }

        return true;
      }

      [HarmonyPrefix, HarmonyPatch(typeof(Smelter), nameof(Smelter.DropAllItems))]
      private static void PrefixDropAllItems(Smelter __instance, ZNetView ___m_nview)
      {
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid()) return;
          if (__instance.IsEternal())
          {
            __instance.SetFuel(__instance.GetOriginalFuelLevel() <= 1f ? 0f : __instance.GetOriginalFuelLevel());
            __instance.SetOriginalFuelLevel(0f);
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }
    }

    #endregion

    #region ShieldGenerator

    [HarmonyPatch(typeof(ShieldGenerator))]
    public class PatchShieldGenerator
    {
      [HarmonyPostfix, HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.Start))]
      private static void PostfixStart(ShieldGenerator __instance, ZNetView ___m_nview)
      {
        try
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
          if (___m_nview == null || !___m_nview.IsValid()) return;
          ___m_nview.Register($"{Main.Guid}_{nameof(ShieldGeneratorExtensions.RPC_EnableEternal)}", __instance.RPC_EnableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(ShieldGeneratorExtensions.RPC_DisableEternal)}", __instance.RPC_DisableEternal);
          ___m_nview.Register($"{Main.Guid}_{nameof(ShieldGeneratorExtensions.RPC_SetFuelLevelMax)}", __instance.RPC_SetFuelLevelMax);
          // __instance.SetIsEternal(__instance.IsEternal());
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.UpdateShield))]
      private static void PrefixUpdateShield(ShieldGenerator __instance, ZNetView ___m_nview)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid() || __instance.m_fuelItems == null || __instance.m_fuelItems.Count < 1f) return;

          if (!___m_nview.HasOwner())
          {
            ___m_nview.ClaimOwnership();
          }

          if (Main.ConfigCheck(__instance.name)) // Is the ShieldGenerator enabled in the mod's config.
          {
            // ShieldGenerator is not tagged Eternal.
            if (!__instance.IsEternal())
            {
              // Tell the owner to tag the ShieldGenerator as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(ShieldGeneratorExtensions.RPC_EnableEternal)}");
            }
          }
          else // The ShieldGenerator is disabled in the mod's config.
          {
            // ShieldGenerator is tagged Eternal.
            if (__instance.IsEternal())
            {
              // Tell the owner to untag the ShieldGenerator as Eternal
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(ShieldGeneratorExtensions.RPC_DisableEternal)}");
            }
          }

          if (__instance.IsEternal() && __instance.GetFuel() <= __instance.m_maxFuel - 1)
          {
            if (Main.Instance.EnableTrace)
            {
              Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [Adding Fuel for {__instance.name}]");
              Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [__instance.GetFuel() : {__instance.GetFuel()}]");
            }

            // Tell the owner to add fuel to the ShieldGenerator
            if (Main.IsVpcInstalled)
            {
              if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [RPC_SetFuelLevelMax for {__instance.name}]");
              ___m_nview.InvokeRPC($"{Main.Guid}_{nameof(ShieldGeneratorExtensions.RPC_SetFuelLevelMax)}");
            }
            else
            {
              if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [RPC_SetFuel for {__instance.name}]");
              __instance.SetFuel(__instance.m_maxFuel); // This calls RPC_SetFuel
            }
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPostfix, HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.OnHoverAddFuel))]
      private static void PostfixOnHoverAddFuel(ShieldGenerator __instance, ZNetView ___m_nview, ref string __result)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return;

          if (__instance.IsEternal())
          {
            __result = $"{__instance.m_name}\n<color={Main.TextColor.Value}><b>$mod_eternal_fire_hover_text</b></color>";
            if (Main.DisplayDebugInfo.Value)
            {
              // ReSharper disable once StringLiteralTypo
              __result += $"\n( $piece_shieldgenerator_add {Mathf.Ceil(__instance.GetFuel())}/{__instance.m_maxFuel} )";
            }

            __result = Localization.instance.Localize(__result);
          }

          if (Main.DisplayDebugInfo.Value)
          {
            __result += $"\nIsEternalZdo: {__instance.GetIsEternal()}";
            __result += $"\nOriginalFuelLevelZdo: {__instance.GetOriginalFuelLevel()}";
            __result += $"\nIsVPCInstalled: {Main.IsVpcInstalled}";
            __result += $"\nm_enableAttack: {__instance.m_enableAttack}";
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }

      [HarmonyPrefix, HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.OnAddFuel))]
      private static bool PrefixOnAddFuel(ShieldGenerator __instance, ZNetView ___m_nview, ref bool __result)
      {
        try
        {
          if (___m_nview == null || !___m_nview.IsValid()) return true;
          if (__instance.IsEternal())
          {
            __result = false;
            return false;
          }
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
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
      try
      {
        if (!fireplace.m_nview.IsValid()) return false;
        return fireplace.GetIsEternal();
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
    }

    /// <summary>
    /// RPC Enable Eternal for Fireplace
    /// </summary>
    /// <param name="fireplace"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this Fireplace fireplace, long sender)
    {
      try
      {
        if (fireplace.m_nview == null
            || !fireplace.m_nview.IsValid()
            || !fireplace.m_nview.IsOwner()
            || !Main.ConfigCheck(fireplace.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");

        var eternal = fireplace.IsEternal(); // Does owner already think it's eternal?
        fireplace.SetIsEternal(true);

        if (!eternal) // Only save the current values if owner thinks the value is being toggled
        {
          // Save the current fuel level so owner can restore it on disable.
          fireplace.SetOriginalFuelLevel(fireplace.GetFuel());
          fireplace.SetFuel(fireplace.m_maxFuel); // Force Max Fuel  
        }
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    /// <summary>
    /// RPC Disable Eternal for Fireplace
    /// </summary>
    /// <param name="fireplace"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this Fireplace fireplace, long sender)
    {
      try
      {
        if (fireplace.m_nview == null
            || !fireplace.m_nview.IsValid()
            || !fireplace.m_nview.IsOwner()
            || Main.ConfigCheck(fireplace.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        var eternal = fireplace.IsEternal(); // Does owner already think it's eternal?
        fireplace.SetIsEternal(false);

        if (eternal) // Only save the current values if owner thinks the value is being toggled
        {
          fireplace.SetFuel(fireplace.GetOriginalFuelLevel() <= 1f ? 0f : fireplace.GetOriginalFuelLevel()); // Restore removed fuel
          fireplace.SetOriginalFuelLevel(0f);                                                                // Reset 
        }
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static float GetOriginalFuelLevel(this Fireplace fireplace)
    {
      if (!fireplace.m_nview.IsValid()) return 0f;
      return fireplace.m_nview.GetZDO().GetFloat(Main.CurrentFuelLevelKey);
    }

    [UsedImplicitly]
    public static void SetOriginalFuelLevel(this Fireplace fireplace, float value)
    {
      if (!fireplace.m_nview.IsValid()) return;
      fireplace.m_nview.GetZDO().Set(Main.CurrentFuelLevelKey, value);
    }

    [UsedImplicitly]
    public static float GetFuel(this Fireplace fireplace)
    {
      if (!fireplace.m_nview.IsValid()) return 0f;
      return fireplace.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
    }

    [UsedImplicitly]
    public static void SetIsEternal(this Fireplace fireplace, bool value)
    {
      try
      {
        if (Main.Instance.EnableTrace)
        {
          Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [value: {value}]");
          Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [m_nview.IsValid: {fireplace.m_nview.IsValid()}]");
          Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [m_nview.IsOwner: {fireplace.m_nview.IsOwner()}]");
        }

        if (!fireplace.m_nview.IsValid()) return;
        if (fireplace.m_nview.IsOwner())
        {
          if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [ZDO.Write(IsEternalKey): value = {value}]");
          fireplace.m_nview.GetZDO().Set(Main.IsEternalKey, value);
        }

        // fireplace.m_infiniteFuel = value;
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static bool GetIsEternal(this Fireplace fireplace)
    {
      if (!fireplace.m_nview.IsValid()) return false;
      var result = fireplace.m_nview.GetZDO().GetBool(Main.IsEternalKey);
      // Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [ZDO.Read(IsEternalKey): result = {result}]");
      return result;
    }

    [UsedImplicitly, Conditional("DEBUG")]
    public static void TriggerLocator(this Fireplace fireplace)
    {
      if (!fireplace.m_nview.IsValid() || !fireplace.m_nview.IsOwner()) return;
      if (fireplace.m_fireworkItemList == null || fireplace.m_fireworkItemList.Length < 4) return;

      fireplace.m_fireworkItemList[3].m_fireworksEffects.Create(fireplace.transform.position
                                                                , Quaternion.Euler(
                                                                                   Random.Range(
                                                                                                -fireplace.m_fireworksMaxRandomAngle
                                                                                                , fireplace.m_fireworksMaxRandomAngle
                                                                                               )
                                                                                   , 0f
                                                                                   , Random.Range(
                                                                                                  -fireplace.m_fireworksMaxRandomAngle
                                                                                                  , fireplace.m_fireworksMaxRandomAngle
                                                                                                 )
                                                                                  ));
      fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);
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
      try
      {
        if (cookingStation.m_nview == null || !cookingStation.m_nview.IsValid()) return false;
        return cookingStation.GetIsEternal();
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
    }

    /// <summary>
    /// Enable Eternal for CookingStation
    /// </summary>
    /// <param name="cookingStation"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this CookingStation cookingStation, long sender)
    {
      try
      {
        if (cookingStation.m_nview == null
            || !cookingStation.m_nview.IsValid()
            || !cookingStation.m_nview.IsOwner()
            || cookingStation.IsEternal()
            || !Main.ConfigCheck(cookingStation.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        // Save the current fuel level so we can restore it on disable.
        cookingStation.SetOriginalFuelLevel(cookingStation.GetFuel());
        cookingStation.SetIsEternal(true);
        cookingStation.SetFuel(cookingStation.m_maxFuel); // Force to Max Fuel
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    /// <summary>
    /// Disable Eternal for CookingStation
    /// </summary>
    /// <param name="cookingStation"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this CookingStation cookingStation, long sender)
    {
      try
      {
        if (cookingStation.m_nview == null
            || !cookingStation.m_nview.IsValid()
            || !cookingStation.m_nview.IsOwner()
            || !cookingStation.IsEternal()
            || Main.ConfigCheck(cookingStation.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        cookingStation.SetIsEternal(false);
        cookingStation.SetFuel(cookingStation.GetOriginalFuelLevel() <= 1f ? 0f : cookingStation.GetOriginalFuelLevel()); // Restore removed fuel
        cookingStation.SetOriginalFuelLevel(0f);                                                                          // Reset 
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    /// <summary>
    /// Set Fuel to Max
    /// </summary>
    /// <param name="cookingStation"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_SetFuelLevelMax(this CookingStation cookingStation, long sender)
    {
      try
      {
        if (cookingStation.m_nview == null
            || !cookingStation.m_nview.IsValid()
            || !cookingStation.m_nview.IsOwner()
            || !cookingStation.IsEternal()
            || !Main.ConfigCheck(cookingStation.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        cookingStation.SetFuel(cookingStation.m_maxFuel);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static float GetOriginalFuelLevel(this CookingStation cookingStation)
    {
      try
      {
        if (!cookingStation.m_nview.IsValid()) return 0f;
        return cookingStation.m_nview.GetZDO().GetFloat(Main.CurrentFuelLevelKey);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return 0f;
    }

    [UsedImplicitly]
    public static void SetOriginalFuelLevel(this CookingStation cookingStation, float value)
    {
      try
      {
        if (!cookingStation.m_nview.IsValid()) return;
        cookingStation.m_nview.GetZDO().Set(Main.CurrentFuelLevelKey, value);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static void SetIsEternal(this CookingStation cookingStation, bool value)
    {
      try
      {
        if (!cookingStation.m_nview.IsValid()) return;
        cookingStation.m_nview.GetZDO().Set(Main.IsEternalKey, value);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static bool GetIsEternal(this CookingStation cookingStation)
    {
      try
      {
        if (!cookingStation.m_nview.IsValid()) return false;
        return cookingStation.m_nview.GetZDO().GetBool(Main.IsEternalKey);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
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
      try
      {
        if (smelter.m_nview == null || !smelter.m_nview.IsValid()) return false;
        return smelter.GetIsEternal();
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
    }

    /// <summary>
    /// Enable Eternal for Smelter
    /// </summary>
    /// <param name="smelter"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this Smelter smelter, long sender)
    {
      try
      {
        if (smelter.m_nview == null
            || !smelter.m_nview.IsValid()
            || !smelter.m_nview.IsOwner()
            || smelter.IsEternal()
            || !Main.ConfigCheck(smelter.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        // Save the current fuel level so we can restore it on disable.
        smelter.SetOriginalFuelLevel(smelter.GetFuel());
        smelter.SetIsEternal(true);
        smelter.SetFuel(smelter.m_maxFuel); // Force Max Fuel
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    /// <summary>
    /// Disable Eternal for Smelter
    /// </summary>
    /// <param name="smelter"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this Smelter smelter, long sender)
    {
      try
      {
        if (smelter.m_nview == null
            || !smelter.m_nview.IsValid()
            || !smelter.m_nview.IsOwner()
            || !smelter.IsEternal()
            || Main.ConfigCheck(smelter.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        smelter.SetIsEternal(false);
        smelter.SetFuel(smelter.GetOriginalFuelLevel() <= 1f ? 0f : smelter.GetOriginalFuelLevel());
        smelter.SetOriginalFuelLevel(0f);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static void RPC_SetFuelLevelMax(this Smelter smelter, long sender)
    {
      try
      {
        if (smelter.m_nview == null
            || !smelter.m_nview.IsValid()
            || !smelter.m_nview.IsOwner()
            || !smelter.IsEternal()
            || !Main.ConfigCheck(smelter.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        smelter.SetFuel(smelter.m_maxFuel);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static float GetOriginalFuelLevel(this Smelter smelter)
    {
      try
      {
        if (!smelter.m_nview.IsValid()) return 0f;
        return smelter.m_nview.GetZDO().GetFloat(Main.CurrentFuelLevelKey);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return 0f;
    }

    [UsedImplicitly]
    public static void SetOriginalFuelLevel(this Smelter smelter, float value)
    {
      try
      {
        if (!smelter.m_nview.IsValid()) return;
        smelter.m_nview.GetZDO().Set(Main.CurrentFuelLevelKey, value);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static void SetIsEternal(this Smelter smelter, bool value)
    {
      try
      {
        if (!smelter.m_nview.IsValid()) return;
        smelter.m_nview.GetZDO().Set(Main.IsEternalKey, value);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static bool GetIsEternal(this Smelter smelter)
    {
      try
      {
        if (!smelter.m_nview.IsValid()) return false;
        return smelter.m_nview.GetZDO().GetBool(Main.IsEternalKey);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
    }
  }

  [UsedImplicitly]
  public static class ShieldGeneratorExtensions
  {
    /// <summary>
    /// Is the current ShieldGenerator Eternal?
    /// </summary>
    /// <param name="shieldGenerator"></param>
    /// <returns>True if the current Smelter is Eternal</returns>
    [UsedImplicitly]
    public static bool IsEternal(this ShieldGenerator shieldGenerator)
    {
      try
      {
        if (shieldGenerator.m_nview == null || !shieldGenerator.m_nview.IsValid()) return false;
        return shieldGenerator.GetIsEternal();
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
    }

    /// <summary>
    /// Enable Eternal for ShieldGenerator
    /// </summary>
    /// <param name="shieldGenerator"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_EnableEternal(this ShieldGenerator shieldGenerator, long sender)
    {
      try
      {
        if (shieldGenerator.m_nview == null
            || !shieldGenerator.m_nview.IsValid()
            || !shieldGenerator.m_nview.IsOwner()
            || shieldGenerator.IsEternal()
            || !Main.ConfigCheck(shieldGenerator.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        // Save the current fuel level so we can restore it on disable.
        shieldGenerator.SetOriginalFuelLevel(shieldGenerator.GetFuel());
        shieldGenerator.SetIsEternal(true);
        shieldGenerator.SetFuel(shieldGenerator.m_maxFuel); // Force Max Fuel
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    /// <summary>
    /// Disable Eternal for ShieldGenerator
    /// </summary>
    /// <param name="shieldGenerator"></param>
    /// <param name="sender"></param>
    [UsedImplicitly]
    public static void RPC_DisableEternal(this ShieldGenerator shieldGenerator, long sender)
    {
      try
      {
        if (shieldGenerator.m_nview == null
            || !shieldGenerator.m_nview.IsValid()
            || !shieldGenerator.m_nview.IsOwner()
            || !shieldGenerator.IsEternal()
            || Main.ConfigCheck(shieldGenerator.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        shieldGenerator.SetIsEternal(false);
        shieldGenerator.SetFuel(shieldGenerator.GetOriginalFuelLevel() <= 1f ? 0f : shieldGenerator.GetOriginalFuelLevel());
        shieldGenerator.SetOriginalFuelLevel(0f);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static void RPC_SetFuelLevelMax(this ShieldGenerator shieldGenerator, long sender)
    {
      try
      {
        if (shieldGenerator.m_nview == null
            || !shieldGenerator.m_nview.IsValid()
            || !shieldGenerator.m_nview.IsOwner()
            || !shieldGenerator.IsEternal()
            || !Main.ConfigCheck(shieldGenerator.name))
        {
          return;
        }

        if (Main.Instance.EnableTrace) Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        shieldGenerator.SetFuel(shieldGenerator.m_maxFuel);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static float GetOriginalFuelLevel(this ShieldGenerator shieldGenerator)
    {
      try
      {
        if (!shieldGenerator.m_nview.IsValid()) return 0f;
        return shieldGenerator.m_nview.GetZDO().GetFloat(Main.CurrentFuelLevelKey);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return 0f;
    }

    [UsedImplicitly]
    public static void SetOriginalFuelLevel(this ShieldGenerator shieldGenerator, float value)
    {
      try
      {
        if (!shieldGenerator.m_nview.IsValid()) return;
        shieldGenerator.m_nview.GetZDO().Set(Main.CurrentFuelLevelKey, value);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static void SetIsEternal(this ShieldGenerator shieldGenerator, bool value)
    {
      try
      {
        if (!shieldGenerator.m_nview.IsValid()) return;
        shieldGenerator.m_nview.GetZDO().Set(Main.IsEternalKey, value);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }
    }

    [UsedImplicitly]
    public static bool GetIsEternal(this ShieldGenerator shieldGenerator)
    {
      try
      {
        if (!shieldGenerator.m_nview.IsValid()) return false;
        return shieldGenerator.m_nview.GetZDO().GetBool(Main.IsEternalKey);
      }
      catch (Exception e)
      {
        Log.Error(Main.Instance, e);
      }

      return false;
    }
  }
}
