using BepInEx;
using BepInEx.Configuration;
using Digitalroot.Modding.Framework.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DMF = Digitalroot.Modding.Framework;

namespace Digitalroot.Valheim.EternalFire
{
  [BepInPlugin(Guid, Name, Version)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [BepInDependency(PluginConfigSection.BoneAppetitGuid, BepInDependency.DependencyFlags.SoftDependency)]
  [NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public partial class Main : BaseUnityPlugin, ITraceableLogging
  {
    #region Configs and Local vars

    private Harmony _harmony;
    public static Main Instance { get; private set; }
    public static readonly int CurrentFuelLevelKey = $"{Guid}_currentFuelLevel".GetStableHashCode();
    public static readonly int IsEternalKey = $"{Guid}_isEternal".GetStableHashCode();
    private static bool? _isVpcInstalled;

    public static bool IsVpcInstalled
    {
      get
      {
        _isVpcInstalled ??= DMF.Utils.ValheimUtils.isModLoaded(PluginConfigSection.VPCGuid);
        return _isVpcInstalled.Value;
      }
    }

    [UsedImplicitly]
    public static ConfigEntry<int> NexusId { get; private set; }

    public static ConfigEntry<string> TextColor { get; private set; }

    public static ConfigEntry<bool> DisplayDebugInfo { get; private set; }

    private static ConfigEntry<bool> _configCandleResin;
    private static ConfigEntry<bool> _configSnowLantern;
    private static ConfigEntry<bool> _configSmokeLessFirePit;
    private static ConfigEntry<bool> _configSmokelessHearth;
    private static ConfigEntry<bool> _configPieceSmokelessBrazierCeiling01;
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
    private static ConfigEntry<bool> _configPieceShieldGenerator;
    private static ConfigEntry<string> _configCustomInstance;
    private static readonly HashSet<string> UnknownPreFabs = new();
    private readonly Dictionary<string, bool> _configCache = new();
    private readonly HashSet<string> _customPrefabConfigCache = new();

    #endregion

    public Main()
    {
      Instance = this;
      #if DEBUG
      EnableTrace = true;
      Log.RegisterSource(Instance); // Enable logging to a './BepInEx/logs' file.
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      Log.Trace(Instance, $"CurrentFuelLevelKey: {CurrentFuelLevelKey}");
      Log.Trace(Instance, $"IsEternalKey: {IsEternalKey}");
      #else
      EnableTrace = false;
      #endif
    }

    [UsedImplicitly]
    private void Awake()
    {
      try
      {
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        NexusId = Config.Bind(PluginConfigSection.General, "NexusID", 2754, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));
        TextColor = Config.Bind(PluginConfigSection.General, "Text Color", "#FFFF0088", new ConfigDescription("#RGBA Color for Eternal Text (#RRGGBBAA)", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdvanced = true }));
        _configCandleResin = Config.Bind(PluginConfigSection.Fireplaces, nameof(DMF.Names.Vanilla.PrefabNames.CandleResin), true, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.CandleResin)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true })); // IsAdminOnly is correct casing.
        _configCandleResin.SettingChanged += SettingsChanged;
        _configSnowLantern = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.SnowLantern, true, new ConfigDescription("Enable Snow Lantern", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configSnowLantern.SettingChanged += SettingsChanged;
        _configFirePit = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.CampFire, true, new ConfigDescription("Enable Campfire", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configFirePit.SettingChanged += SettingsChanged;
        _configIronFirePit = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.IronFirePit, true, new ConfigDescription("Enable Iron Fire Pit", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configIronFirePit.SettingChanged += SettingsChanged;
        _configBonfire = Config.Bind(PluginConfigSection.Fireplaces, nameof(DMF.Names.Vanilla.PrefabNames.Bonfire), true, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.Bonfire)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configBonfire.SettingChanged += SettingsChanged;
        _configPieceWallTorch = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.Sconce, true, new ConfigDescription("Enable Sconce", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceWallTorch.SettingChanged += SettingsChanged;
        _configPieceGroundTorch = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.StandingIronTorch, true, new ConfigDescription("Enable Standing Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceGroundTorch.SettingChanged += SettingsChanged;
        _configPieceGroundTorchWood = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.StandingWoodTorch, true, new ConfigDescription("Enable Standing Wood Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceGroundTorchWood.SettingChanged += SettingsChanged;
        _configPieceGroundTorchGreen = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.StandingGreenBurningIronTorch, true, new ConfigDescription("Enable Standing Green Burning Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceGroundTorchGreen.SettingChanged += SettingsChanged;
        _configPieceGroundTorchBlue = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.StandingBlueBurningIronTorch, true, new ConfigDescription("Enable Standing Blue Burning Iron Torch", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceGroundTorchBlue.SettingChanged += SettingsChanged;
        _configPieceBrazierFloor01 = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.StandingBrazier, true, new ConfigDescription("Enable Standing Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceBrazierFloor01.SettingChanged += SettingsChanged;
        _configPieceBrazierFloor02 = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.StandingBlueBrazier, true, new ConfigDescription("Enable Standing Blue Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceBrazierFloor02.SettingChanged += SettingsChanged;
        _configPieceBrazierCeiling01 = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.HangingBrazier, true, new ConfigDescription("Enable Hanging Brazier", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceBrazierCeiling01.SettingChanged += SettingsChanged;
        _configPieceJackoTurnip = Config.Bind(PluginConfigSection.Fireplaces, PluginConfigSection.JackOTurnip, true, new ConfigDescription("Enable Jack-o-Turnip", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceJackoTurnip.SettingChanged += SettingsChanged;
        _configHearth = Config.Bind(PluginConfigSection.Fireplaces, nameof(DMF.Names.Vanilla.PrefabNames.Hearth), true, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.Hearth)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configHearth.SettingChanged += SettingsChanged;
        _configPieceBathtub = Config.Bind(PluginConfigSection.Smelters, PluginConfigSection.HotTub, true, new ConfigDescription("Enable Hot Tub", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceBathtub.SettingChanged += SettingsChanged;
        _configPieceOven = Config.Bind(PluginConfigSection.CookingStations, PluginConfigSection.StoneOven, true, new ConfigDescription("Enable Stone Oven", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceOven.SettingChanged += SettingsChanged;
        _configSmelter = Config.Bind(PluginConfigSection.Smelters, nameof(DMF.Names.Vanilla.PrefabNames.Smelter), false, new ConfigDescription($"Enable {nameof(DMF.Names.Vanilla.PrefabNames.Smelter)}", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configSmelter.SettingChanged += SettingsChanged;
        _configBlastFurnace = Config.Bind(PluginConfigSection.Smelters, PluginConfigSection.BlastFurnace, false, new ConfigDescription("Enable Blast Furnace", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configBlastFurnace.SettingChanged += SettingsChanged;
        _configEitrRefinery = Config.Bind(PluginConfigSection.Smelters, PluginConfigSection.EitrRefinery, false, new ConfigDescription("Enable Eitr Refinery", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configEitrRefinery.SettingChanged += SettingsChanged;
        _configPieceShieldGenerator = Config.Bind(PluginConfigSection.ShieldGenerators, PluginConfigSection.ShieldGenerator, false, new ConfigDescription("Enable Shield Generator", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configPieceShieldGenerator.SettingChanged += SettingsChanged;
        _configCustomInstance = Config.Bind(PluginConfigSection.Custom, PluginConfigSection.CustomPrefabs, "", new ConfigDescription("A comma-separated list of prefab names", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdminOnly = true }));
        _configCustomInstance.SettingChanged += SettingsChanged;

        #region com.rockerkitten.boneappetit configs

        var isBoneAppetitLoaded = DMF.Utils.ValheimUtils.isModLoaded(PluginConfigSection.BoneAppetitGuid);
        _configSmokeLessFirePit = Config.Bind(PluginConfigSection.BoneAppetit, nameof(PluginConfigSection.SmokelessFirePit), true, new ConfigDescription($"Enable {PluginConfigSection.SmokelessFirePitName}", null, new ConfigurationManagerAttributes { Browsable = isBoneAppetitLoaded, ReadOnly = false, IsAdminOnly = true }));
        _configSmokeLessFirePit.SettingChanged += SettingsChanged;
        _configSmokelessHearth = Config.Bind(PluginConfigSection.BoneAppetit, nameof(PluginConfigSection.SmokelessHearth), true, new ConfigDescription($"Enable {PluginConfigSection.SmokelessHearthName}", null, new ConfigurationManagerAttributes { Browsable = isBoneAppetitLoaded, ReadOnly = false, IsAdminOnly = true }));
        _configSmokelessHearth.SettingChanged += SettingsChanged;
        _configPieceSmokelessBrazierCeiling01 = Config.Bind(PluginConfigSection.BoneAppetit, nameof(PluginConfigSection.SmokelessHangingBrazier), true, new ConfigDescription($"Enable {PluginConfigSection.SmokelessHangingBrazierName}", null, new ConfigurationManagerAttributes { Browsable = isBoneAppetitLoaded, ReadOnly = false, IsAdminOnly = true }));
        _configPieceSmokelessBrazierCeiling01.SettingChanged += SettingsChanged;

        #endregion

        DisplayDebugInfo = Config.Bind(PluginConfigSection.Advanced, nameof(DisplayDebugInfo), false, new ConfigDescription($"Enable to display debug info on mouseover", null, new ConfigurationManagerAttributes { Browsable = true, ReadOnly = false, IsAdvanced = true }));
        SynchronizationManager.OnConfigurationSynchronized += SynchronizationManager_OnConfigurationSynchronized;

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
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        _harmony?.UnpatchSelf();
        SynchronizationManager.OnConfigurationSynchronized -= SynchronizationManager_OnConfigurationSynchronized;
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    #region Helper Methods

    public static bool ConfigCheck(string instanceName)
    {
      if (Instance._configCache.Count == 0) // Should only trigger on init.
      {
        UpdateConfigCache();
        UpdateCustomPrefabConfigCache();
      }

      var eternalFuel = false;
      instanceName = instanceName.Replace("(Clone)", string.Empty);

      if (Instance._configCache.TryGetValue(instanceName, out var value))
      {
        eternalFuel = value;
      }
      else if (Instance._customPrefabConfigCache.Contains(instanceName))
      {
        eternalFuel = true;
      }
      else if (UnknownPreFabs.Add(instanceName)) // Unknown
      {
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}[{instanceName}] Unknown");
      }

      return eternalFuel;
    }

    private void SettingsChanged(object sender, EventArgs e)
    {
      try
      {
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        if (sender is ConfigEntryBase entry)
        {
          switch (entry.Definition.Key)
          {
            case PluginConfigSection.CampFire:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.FirePit, _configFirePit.Value);
              break;

            case PluginConfigSection.IronFirePit:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.FirePitIron, _configIronFirePit.Value);
              break;

            case nameof(DMF.Names.Vanilla.PrefabNames.Bonfire):
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Bonfire, _configBonfire.Value);
              break;

            case nameof(DMF.Names.Vanilla.PrefabNames.Hearth):
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Hearth, _configHearth.Value);
              break;

            case PluginConfigSection.Sconce:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceWalltorch, _configPieceWallTorch.Value);
              break;

            case PluginConfigSection.StandingIronTorch:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorch, _configPieceGroundTorch.Value);
              break;

            case PluginConfigSection.StandingWoodTorch:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorchWood, _configPieceGroundTorchWood.Value);
              break;

            case PluginConfigSection.StandingGreenBurningIronTorch:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorchGreen, _configPieceGroundTorchGreen.Value);
              break;

            case PluginConfigSection.StandingBlueBurningIronTorch:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorchBlue, _configPieceGroundTorchBlue.Value);
              break;

            case PluginConfigSection.StandingBrazier:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor01, _configPieceBrazierFloor01.Value);
              break;

            case PluginConfigSection.StandingBlueBrazier:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor02, _configPieceBrazierFloor02.Value);
              break;

            case PluginConfigSection.HangingBrazier:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBrazierceiling01, _configPieceBrazierCeiling01.Value);
              break;

            case PluginConfigSection.JackOTurnip:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceJackoturnip, _configPieceJackoTurnip.Value);
              break;

            case PluginConfigSection.StoneOven:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceOven, _configPieceOven.Value);
              break;

            case nameof(DMF.Names.Vanilla.PrefabNames.Smelter):
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Smelter, _configSmelter.Value);
              break;

            case PluginConfigSection.BlastFurnace:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Blastfurnace, _configBlastFurnace.Value);
              break;

            case PluginConfigSection.EitrRefinery:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Eitrrefinery, _configEitrRefinery.Value);
              break;

            case PluginConfigSection.HotTub:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBathtub, _configPieceBathtub.Value);
              break;

            case nameof(DMF.Names.Vanilla.PrefabNames.CandleResin):
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.CandleResin, _configCandleResin.Value);
              break;

            case PluginConfigSection.SnowLantern:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceSnowlantern, _configSnowLantern.Value);
              break;

            case PluginConfigSection.ShieldGenerator:
              UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceShieldgenerator, _configPieceShieldGenerator.Value);
              break;

            case nameof(PluginConfigSection.SmokelessHearth):
              UpdateConfigCache(PluginConfigSection.SmokelessHearth, _configSmokelessHearth.Value);
              break;

            case nameof(PluginConfigSection.SmokelessFirePit):
              UpdateConfigCache(PluginConfigSection.SmokelessFirePit, _configSmokeLessFirePit.Value);
              break;

            case nameof(PluginConfigSection.SmokelessHangingBrazier):
              UpdateConfigCache(PluginConfigSection.SmokelessHangingBrazier, _configPieceSmokelessBrazierCeiling01.Value);
              break;

            case PluginConfigSection.CustomPrefabs:
              UpdateCustomPrefabConfigCache();
              break;

            default:
              if (Instance.EnableTrace) Log.Trace(Instance, $"Changed: [{entry.Definition.Section}] {entry.Definition.Key} = {entry.BoxedValue}");
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(Instance, ex);
      }
    }

    private void SynchronizationManager_OnConfigurationSynchronized(object sender, ConfigurationSynchronizationEventArgs e)
    {
      if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      UpdateConfigCaches();
    }

    private void UpdateConfigCaches()
    {
      try
      {
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        UpdateConfigCache();
        UpdateCustomPrefabConfigCache();
      }
      catch (Exception ex)
      {
        Log.Error(Instance, ex);
      }
    }

    private static void UpdateConfigCache()
    {
      if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.FirePit, _configFirePit.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.FirePitIron, _configIronFirePit.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Bonfire, _configBonfire.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Hearth, _configHearth.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceWalltorch, _configPieceWallTorch.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorch, _configPieceGroundTorch.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorchWood, _configPieceGroundTorchWood.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorchGreen, _configPieceGroundTorchGreen.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceGroundtorchBlue, _configPieceGroundTorchBlue.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor01, _configPieceBrazierFloor01.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBrazierfloor02, _configPieceBrazierFloor02.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBrazierceiling01, _configPieceBrazierCeiling01.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceJackoturnip, _configPieceJackoTurnip.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceOven, _configPieceOven.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Smelter, _configSmelter.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Blastfurnace, _configBlastFurnace.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.Eitrrefinery, _configEitrRefinery.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceBathtub, _configPieceBathtub.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.CandleResin, _configCandleResin.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceSnowlantern, _configSnowLantern.Value);
      UpdateConfigCache(DMF.Names.Vanilla.PrefabNames.PieceShieldgenerator, _configPieceShieldGenerator.Value);
      UpdateConfigCache(PluginConfigSection.SmokelessHearth, _configSmokelessHearth.Value);
      UpdateConfigCache(PluginConfigSection.SmokelessFirePit, _configSmokeLessFirePit.Value);
      UpdateConfigCache(PluginConfigSection.SmokelessHangingBrazier, _configPieceSmokelessBrazierCeiling01.Value);
    }

    private static void UpdateCustomPrefabConfigCache()
    {
      try
      {
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");

        Instance._customPrefabConfigCache.Clear();
        if (!string.IsNullOrWhiteSpace(_configCustomInstance?.Value))
        {
          foreach (var entry in _configCustomInstance.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
          {
            var prefab = entry.Trim();
            if (prefab.Length > 0)
            {
              Instance._customPrefabConfigCache.Add(prefab);
              if (Instance.EnableTrace)
              {
                Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [Added {prefab}]");
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    private static void UpdateConfigCache(string prefabName, bool value)
    {
      try
      {
        if (Instance.EnableTrace) Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name} [{prefabName},{value}]");
        Instance._configCache[prefabName] = value;
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    #endregion

    #region Implementation of ITraceableLogging

    /// <inheritdoc />
    public string Source => Namespace;

    /// <inheritdoc />
    public bool EnableTrace { get; }

    #endregion
  }
}
