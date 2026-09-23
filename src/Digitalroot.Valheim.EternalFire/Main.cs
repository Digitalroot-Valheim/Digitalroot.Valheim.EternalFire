using BepInEx;
using BepInEx.Configuration;
using DMF = Digitalroot.Modding.Framework;
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
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public partial class Main : BaseUnityPlugin, DMF.Logging.ITraceableLogging
  {
    private Harmony _harmony;
    public static Main Instance;

    // [UsedImplicitly] public static ConfigEntry<int> NexusId;
    public static ConfigEntry<int> NexusId { get; private set; }
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
      DMF.Logging.Log.RegisterSource(Instance);
      #else
      EnableTrace = false;
      #endif
      DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
    }

    [UsedImplicitly]
    private void Awake()
    {
      try
      {
        DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        NexusId = Config.Bind(PluginConfigSection.General, "NexusID", 2754, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));
        config_fire_pit = Config.Bind<bool>(PluginConfigSection.Fireplaces, "CampFire", true, new ConfigDescription("Enable Campfire", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_iron_fire_pit = Config.Bind<bool>(PluginConfigSection.Fireplaces, "IronFirePit", true, new ConfigDescription("Enable Iron Fire Pit", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_bonfire = Config.Bind<bool>(PluginConfigSection.Fireplaces, "Bonfire", true, new ConfigDescription("Enable Bonfire", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_walltorch = Config.Bind<bool>(PluginConfigSection.Fireplaces, "Sconce", true, new ConfigDescription("Enable Sconce", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_groundtorch = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingIronTorch", true, new ConfigDescription("Enable Standing Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_groundtorch_wood = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingWoodTorch", true, new ConfigDescription("Enable Standing Wood Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_groundtorch_green = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingGreenBurningIronTorch", true, new ConfigDescription("Enable Standing Green Burning Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_groundtorch_blue = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingBlueBurningIronTorch", true, new ConfigDescription("Enable Standing Blue Burning Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_brazierfloor01 = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingBrazier", true, new ConfigDescription("Enable Standing Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_brazierfloor02 = Config.Bind<bool>(PluginConfigSection.Fireplaces, "StandingBlueBrazier", true, new ConfigDescription("Enable Standing Blue Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_brazierceiling01 = Config.Bind<bool>(PluginConfigSection.Fireplaces, "HangingBrazier", true, new ConfigDescription("Enable Hanging Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_jackoturnip = Config.Bind<bool>(PluginConfigSection.Fireplaces, "JackOTurnip", true, new ConfigDescription("Enable Jack-o-Turnip", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_hearth = Config.Bind<bool>(PluginConfigSection.Fireplaces, "Hearth", true, new ConfigDescription("Enable Hearth", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_bathtub = Config.Bind<bool>(PluginConfigSection.Smelters, "HotTub", true, new ConfigDescription("Enable Hot Tub", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_piece_oven = Config.Bind<bool>(PluginConfigSection.CookingStations, "StoneOven", true, new ConfigDescription("Enable Stone Oven", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_smelter = Config.Bind<bool>(PluginConfigSection.Smelters, "Smelter", false, new ConfigDescription("Enable Smelter", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_blastfurnace = Config.Bind<bool>(PluginConfigSection.Smelters, "BlastFurnace", false, new ConfigDescription("Enable Blast Furnace", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_eitrrefinery = Config.Bind<bool>(PluginConfigSection.Smelters, "EitrRefinery", false, new ConfigDescription("Enable Eitr Refinery", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        config_custom_instance = Config.Bind<string>(PluginConfigSection.Custom, "CustomPrefabs", "", new ConfigDescription("A comma-separated list of prefab names", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));

        _harmony = Harmony.CreateAndPatchAll(typeof(Main).Assembly, Guid);
      }
      catch (Exception e)
      {
        DMF.Logging.Log.Error(Instance, e);
      }
    }

    [UsedImplicitly]
    private void OnDestroy()
    {
      try
      {
        DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        _harmony?.UnpatchSelf();
      }
      catch (Exception e)
      {
        DMF.Logging.Log.Error(Instance, e);
      }
    }

    // ReSharper disable once IdentifierTypo
    public static void Refuel(ZNetView znview)
    {
      Task.Delay(33).Wait();
      znview.InvokeRPC("AddFuel");
    }

    public static bool ConfigCheck(string instanceName)
    {
      bool EternalFuel = false;
      instanceName = instanceName.Replace("(Clone)", string.Empty); 
      switch (instanceName)
      {
        case DMF.Names.Vanilla.PrefabNames.FirePit:
          EternalFuel = config_fire_pit.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.FirePitIron:
          EternalFuel = config_iron_fire_pit.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Bonfire:
          EternalFuel = config_bonfire.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Hearth:
          EternalFuel = config_hearth.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceWalltorch:
          EternalFuel = config_piece_walltorch.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorch:
          EternalFuel = config_piece_groundtorch.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorchWood:
          EternalFuel = config_piece_groundtorch_wood.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorchGreen:
          EternalFuel = config_piece_groundtorch_green.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorchBlue:
          EternalFuel = config_piece_groundtorch_blue.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor01:
          EternalFuel = config_piece_brazierfloor01.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor02:
          EternalFuel = config_piece_brazierfloor02.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBrazierceiling01:
          EternalFuel = config_piece_brazierceiling01.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceJackoturnip:
          EternalFuel = config_piece_jackoturnip.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceOven:
          EternalFuel = config_piece_oven.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Smelter:
          EternalFuel = config_smelter.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Blastfurnace:
          EternalFuel = config_blastfurnace.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Eitrrefinery:
          EternalFuel = config_eitrrefinery.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBathtub:
          EternalFuel = config_piece_bathtub.Value;
          break;

        default:
          DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}[{instanceName}] Unknown");
          break;
      }

      if (config_custom_instance.Value.Split(',').Contains(instanceName))
      {
        EternalFuel = true;
      }

      DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}[{instanceName}] {EternalFuel}");

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
