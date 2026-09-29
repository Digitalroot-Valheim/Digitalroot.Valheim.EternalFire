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

    [UsedImplicitly]
    public static ConfigEntry<int> NexusId { get; private set; }
    public static ConfigEntry<string> TextColor { get; private set; }
    private static ConfigEntry<bool> _configCandleResin;
    private static ConfigEntry<bool> _configSnowLantern;
    private static ConfigEntry<bool> _configFirePit;
    private static ConfigEntry<bool> _configIronFirePit;
    private static ConfigEntry<bool> _configBonfire;
    private static ConfigEntry<bool> _configHearth;
    private static ConfigEntry<bool> _configPieceWallTorch;
    private static ConfigEntry<bool> _configPieceGroundTorch;
    private static ConfigEntry<bool> _configPieceGroundTorchWood;
    private static ConfigEntry<bool> _configPieceGroundTorchGreen;
    private static ConfigEntry<bool> _configPieceGroundTorchBlue;
    private static ConfigEntry<bool> _configPieceBrazierFloor01;
    private static ConfigEntry<bool> _configPieceBrazierFloor02;
    private static ConfigEntry<bool> _configPieceBrazierCeiling01;
    private static ConfigEntry<bool> _configPieceJackoTurnip;
    private static ConfigEntry<bool> _configPieceOven;
    private static ConfigEntry<bool> _configSmelter;
    private static ConfigEntry<bool> _configBlastFurnace;
    private static ConfigEntry<bool> _configEitrRefinery;
    private static ConfigEntry<bool> _configPieceBathtub;
    private static ConfigEntry<string> _configCustomInstance;

    public Main()
    {
      Instance = this;
      #if DEBUG
      EnableTrace = true;
      DMF.Logging.Log.RegisterSource(Instance); // Enable logging to a './BepInEx/logs' file.
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
        TextColor = Config.Bind(PluginConfigSection.General, "Text Color", "#FFFF0088", new ConfigDescription("#RGBA Color for Eternal Text (#RRGGBBAA)", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdvanced = true}));
        _configCandleResin = Config.Bind(PluginConfigSection.Fireplaces,  nameof(DMF.Names.Vanilla.PrefabNames.CandleResin), true, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.CandleResin)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configSnowLantern = Config.Bind(PluginConfigSection.Fireplaces,  "SnowLantern", true, new ConfigDescription("Enable Snow Lantern", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configFirePit = Config.Bind(PluginConfigSection.Fireplaces, "CampFire", true, new ConfigDescription("Enable Campfire", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configIronFirePit = Config.Bind(PluginConfigSection.Fireplaces, "IronFirePit", true, new ConfigDescription("Enable Iron Fire Pit", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configBonfire = Config.Bind(PluginConfigSection.Fireplaces, nameof(DMF.Names.Vanilla.PrefabNames.Bonfire), true, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.Bonfire)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceWallTorch = Config.Bind(PluginConfigSection.Fireplaces, "Sconce", true, new ConfigDescription("Enable Sconce", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceGroundTorch = Config.Bind(PluginConfigSection.Fireplaces, "StandingIronTorch", true, new ConfigDescription("Enable Standing Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceGroundTorchWood = Config.Bind(PluginConfigSection.Fireplaces, "StandingWoodTorch", true, new ConfigDescription("Enable Standing Wood Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceGroundTorchGreen = Config.Bind(PluginConfigSection.Fireplaces, "StandingGreenBurningIronTorch", true, new ConfigDescription("Enable Standing Green Burning Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceGroundTorchBlue = Config.Bind(PluginConfigSection.Fireplaces, "StandingBlueBurningIronTorch", true, new ConfigDescription("Enable Standing Blue Burning Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceBrazierFloor01 = Config.Bind(PluginConfigSection.Fireplaces, "StandingBrazier", true, new ConfigDescription("Enable Standing Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceBrazierFloor02 = Config.Bind(PluginConfigSection.Fireplaces, "StandingBlueBrazier", true, new ConfigDescription("Enable Standing Blue Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceBrazierCeiling01 = Config.Bind(PluginConfigSection.Fireplaces, "HangingBrazier", true, new ConfigDescription("Enable Hanging Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceJackoTurnip = Config.Bind(PluginConfigSection.Fireplaces, "JackOTurnip", true, new ConfigDescription("Enable Jack-o-Turnip", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configHearth = Config.Bind(PluginConfigSection.Fireplaces, nameof(DMF.Names.Vanilla.PrefabNames.Hearth), true, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.Hearth)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceBathtub = Config.Bind(PluginConfigSection.Smelters, "HotTub", true, new ConfigDescription("Enable Hot Tub", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configPieceOven = Config.Bind(PluginConfigSection.CookingStations, "StoneOven", true, new ConfigDescription("Enable Stone Oven", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configSmelter = Config.Bind(PluginConfigSection.Smelters, nameof(DMF.Names.Vanilla.PrefabNames.Smelter), false, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.Smelter)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configBlastFurnace = Config.Bind(PluginConfigSection.Smelters, "BlastFurnace", false, new ConfigDescription("Enable Blast Furnace", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configEitrRefinery = Config.Bind(PluginConfigSection.Smelters, "EitrRefinery", false, new ConfigDescription("Enable Eitr Refinery", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));
        _configCustomInstance = Config.Bind(PluginConfigSection.Custom, "CustomPrefabs", "", new ConfigDescription("A comma-separated list of prefab names", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, isAdminOnly = true }));

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

    public static bool ConfigCheck(string instanceName)
    {
      bool EternalFuel = false;
      instanceName = instanceName.Replace("(Clone)", string.Empty); 
      switch (instanceName)
      {
        case DMF.Names.Vanilla.PrefabNames.FirePit:
          EternalFuel = _configFirePit.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.FirePitIron:
          EternalFuel = _configIronFirePit.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Bonfire:
          EternalFuel = _configBonfire.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Hearth:
          EternalFuel = _configHearth.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceWalltorch:
          EternalFuel = _configPieceWallTorch.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorch:
          EternalFuel = _configPieceGroundTorch.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorchWood:
          EternalFuel = _configPieceGroundTorchWood.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorchGreen:
          EternalFuel = _configPieceGroundTorchGreen.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceGroundtorchBlue:
          EternalFuel = _configPieceGroundTorchBlue.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor01:
          EternalFuel = _configPieceBrazierFloor01.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor02:
          EternalFuel = _configPieceBrazierFloor02.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBrazierceiling01:
          EternalFuel = _configPieceBrazierCeiling01.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceJackoturnip:
          EternalFuel = _configPieceJackoTurnip.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceOven:
          EternalFuel = _configPieceOven.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Smelter:
          EternalFuel = _configSmelter.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Blastfurnace:
          EternalFuel = _configBlastFurnace.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.Eitrrefinery:
          EternalFuel = _configEitrRefinery.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceBathtub:
          EternalFuel = _configPieceBathtub.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.CandleResin:
          EternalFuel = _configCandleResin.Value;
          break;

        case DMF.Names.Vanilla.PrefabNames.PieceSnowlantern:
          EternalFuel = _configSnowLantern.Value;
          break;

        default:
          DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}[{instanceName}] Unknown");
          break;
      }

      if (_configCustomInstance.Value.Split(',').Contains(instanceName))
      {
        EternalFuel = true;
      }

      // DMF.Logging.Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}[{instanceName}] {EternalFuel}");

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
