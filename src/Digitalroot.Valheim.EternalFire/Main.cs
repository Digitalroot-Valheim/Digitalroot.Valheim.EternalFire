using BepInEx;
using BepInEx.Configuration;
using Digitalroot.Valheim.Common;
using Digitalroot.Valheim.Common.Names;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Digitalroot.Valheim.EternalFire
{
  [BepInPlugin(Guid, Name, Version)]
  [BepInDependency(Jotunn.Main.ModGuid, "2.11.5")]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public partial class Main : BaseUnityPlugin, ITraceableLogging
  {
    private Harmony _harmony;
    public static Main Instance;

    // [UsedImplicitly] public static ConfigEntry<int> NexusId;
    public  static ConfigEntry<int> NexusId { get; private set; }
    private static ConfigEntry<bool> config_fire_pit;
    private static ConfigEntry<bool> config_iron_fire_pit;
    private static ConfigEntry<bool> config_bonfire;
    private static ConfigEntry<bool> config_hearth;
    private static ConfigEntry<bool> config_piece_walltorch;
    private static ConfigEntry<bool> config_piece_groundtorch;
    private static ConfigEntry<bool> config_piece_groundtorch_wood;
    private static ConfigEntry<bool> config_piece_groundtorch_green;
    private static ConfigEntry<bool> config_piece_groundtorch_blue;
    private static ConfigEntry<bool> config_piece_brazierfloor01;
    private static ConfigEntry<bool> config_piece_brazierfloor02;
    private static ConfigEntry<bool> config_piece_brazierceiling01;
    private static ConfigEntry<bool> config_piece_jackoturnip;
    private static ConfigEntry<bool> config_piece_oven;
    private static ConfigEntry<bool> config_smelter;
    private static ConfigEntry<bool> config_blastfurnace;
    private static ConfigEntry<bool> config_eitrrefinery;
    private static ConfigEntry<bool> config_piece_bathtub;
    private static ConfigEntry<string> config_custom_instance;

    public Main()
    {
      Instance = this;
      #if DEBUG
      EnableTrace = true;
      Log.RegisterSource(Instance);
      #else
      EnableTrace = false;
      #endif
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
    }

    [UsedImplicitly]
    private void Awake()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        NexusId = Config.Bind(PluginConfigSection.General, "NexusID", 2754, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));
        config_fire_pit = Config.Bind<bool>(PluginConfigSection.Fireplaces, "CampFire", true, "Enable Campfire");
        config_iron_fire_pit = Config.Bind<bool>(PluginConfigSection.Fireplaces, "IronFirePit", true, "Enable Iron Fire Pit");
        config_bonfire = Config.Bind<bool>(PluginConfigSection.Fireplaces, "Bonfire", true, "Enable Bonfire");
        config_piece_walltorch = Config.Bind<bool>(PluginConfigSection.Fireplaces, "Sconce", true, "Enable Sconce");
        config_piece_groundtorch = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingIronTorch", true, "Enable Standing Iron Torch");
        config_piece_groundtorch_wood = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingWoodTorch", true, "Enable Standing Wood Torch");
        config_piece_groundtorch_green = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingGreenBurningIronTorch", true, "Enable Standing Green Burning Iron Torch");
        config_piece_groundtorch_blue = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingBlueBurningIronTorch", true, "Enable Standing Blue Burning Iron Torch");
        config_piece_brazierfloor01 = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingBrazier", true, "Enable Standing Brazier");
        config_piece_brazierfloor02 = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingBlueBrazier", true, "Enable Standing Blue Brazier");
        config_piece_brazierceiling01 = Config.Bind<bool>(PluginConfigSection.Fireplaces, "HangingBrazier", true, "Enable Hanging Brazier");
        config_piece_jackoturnip = Config.Bind<bool>(PluginConfigSection.Fireplaces, "JackOTurnip", true, "Enable Jack-o-Turnip");
        config_hearth = Config.Bind<bool>(PluginConfigSection.Fireplaces, "Hearth", true, "Enable Hearth");
        config_piece_bathtub = Config.Bind<bool>(PluginConfigSection.Fireplaces, "HotTub", true, "Enable Hot Tub");
        config_piece_oven = Config.Bind<bool>(PluginConfigSection.CookingStations, "StoneOven", true, "Enable Stone Oven");
        config_smelter = Config.Bind<bool>(PluginConfigSection.Smelters, "Smelter", false, "Enable Smelter");
        config_blastfurnace = Config.Bind<bool>(PluginConfigSection.Smelters, "BlastFurnace", false, "Enable Blast Furnace");
        config_eitrrefinery = Config.Bind<bool>(PluginConfigSection.Smelters, "EitrRefinery", false, "Enable Eitr Refinery");
        config_custom_instance = Config.Bind<string>(PluginConfigSection.Custom, "CustomPrefabs", "", "A comma-separated list of prefab names");

        _harmony = Harmony.CreateAndPatchAll(typeof(Main).Assembly, Guid);
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }
    
    [UsedImplicitly]
    private void OnDestroy()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        _harmony?.UnpatchSelf();
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    public static async void Refuel(ZNetView znview)
    {
      await Task.Delay(33);
      znview.InvokeRPC("AddFuel");
    }

    public static bool ConfigCheck(string instanceName)
    {
      bool EternalFuel = false;
      switch (instanceName.Replace("(Clone)", string.Empty))
      {
        case Common.Names.Vanilla.PrefabNames.FirePit:
          EternalFuel = config_fire_pit.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.FirePitIron:
          EternalFuel = config_iron_fire_pit.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.Bonfire:
          EternalFuel = config_bonfire.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.Hearth:
          EternalFuel = config_hearth.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceWalltorch:
          EternalFuel = config_piece_walltorch.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceGroundtorch:
          EternalFuel = config_piece_groundtorch.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceGroundtorchWood:
          EternalFuel = config_piece_groundtorch_wood.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceGroundtorchGreen:
          EternalFuel = config_piece_groundtorch_green.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceGroundtorchBlue:
          EternalFuel = config_piece_groundtorch_blue.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceBrazierfloor01:
          EternalFuel = config_piece_brazierfloor01.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceBrazierfloor02:
          EternalFuel = config_piece_brazierfloor02.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceBrazierceiling01:
          EternalFuel = config_piece_brazierceiling01.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceJackoturnip:
          EternalFuel = config_piece_jackoturnip.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceOven:
          EternalFuel = config_piece_oven.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.Smelter:
          EternalFuel = config_smelter.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.Blastfurnace:
          EternalFuel = config_blastfurnace.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.Eitrrefinery:
          EternalFuel = config_eitrrefinery.Value;
          break;

        case Common.Names.Vanilla.PrefabNames.PieceBathtub:
          EternalFuel = config_piece_bathtub.Value;
          break;
      }

      if (config_custom_instance.Value.Split(',').Contains(instanceName.Remove(instanceName.Length - 7))) EternalFuel = true;
      return EternalFuel;
    }

    #region Implementation of ITraceableLogging

    /// <inheritdoc />
    public string Source => Namespace;

    /// <inheritdoc />
    public bool EnableTrace { get; }

    #endregion
  }
}
