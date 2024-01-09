using BepInEx;
using BepInEx.Configuration;
using Digitalroot.Valheim.Common;
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
    private readonly Harmony harmony = new Harmony(Guid);
    public  static ConfigEntry<int> NexusId { get; private set; }
    private static ConfigEntry<bool> config_fire_pit;
    private static ConfigEntry<bool> config_bonfire;
    private static ConfigEntry<bool> config_hearth;
    private static ConfigEntry<bool> config_piece_walltorch;
    private static ConfigEntry<bool> config_piece_groundtorch;
    private static ConfigEntry<bool> config_piece_groundtorch_wood;
    private static ConfigEntry<bool> config_piece_groundtorch_green;
    private static ConfigEntry<bool> config_piece_groundtorch_blue;
    private static ConfigEntry<bool> config_piece_brazierfloor01;
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
        NexusId = Config.Bind("General", "NexusID", 0000, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));
        config_fire_pit = Config.Bind<bool>("Fireplaces", "CampFire", true, "Enable Campfire");
        config_bonfire = Config.Bind<bool>("Fireplaces", "Bonfire", true, "Enable Bonfire");
        config_piece_walltorch = Config.Bind<bool>("Fireplaces", "Sconce", true, "Enable Sconce");
        config_piece_groundtorch = Config.Bind<bool>("Fireplaces", "StandingIronTorch", true, "Enable Standing Iron Torch");
        config_piece_groundtorch_wood = Config.Bind<bool>("Fireplaces", "StandingWoodTorch", true, "Enable Standing Wood Torch");
        config_piece_groundtorch_green = Config.Bind<bool>("Fireplaces", "StandingGreenBurningIronTorch", true, "Enable Standing Green Burning Iron Torch");
        config_piece_groundtorch_blue = Config.Bind<bool>("Fireplaces", "StandingBlueBurningIronTorch", true, "Enable Standing Blue Burning Iron Torch");
        config_piece_brazierfloor01 = Config.Bind<bool>("Fireplaces", "StandingBrazier", true, "Enable Standing Brazier");
        config_piece_brazierceiling01 = Config.Bind<bool>("Fireplaces", "HangingBrazier", true, "Enable Hanging Brazier");
        config_piece_jackoturnip = Config.Bind<bool>("Fireplaces", "JackOTurnip", true, "Enable Jack-o-turnip");
        config_hearth = Config.Bind<bool>("Fireplaces", "Hearth", true, "Enable Hearth");
        config_piece_bathtub = Config.Bind<bool>("Fireplaces", "HotTub", true, "Enable Hot tub");
        config_piece_oven = Config.Bind<bool>("CookingStations", "StoneOven", true, "Enable Stone oven");
        config_smelter = Config.Bind<bool>("Smelters", "Smelter", false, "Enable Smelter");
        config_blastfurnace = Config.Bind<bool>("Smelters", "BlastFurnace", false, "Enable Blast furnace");
        config_eitrrefinery = Config.Bind<bool>("Smelters", "EitrRefinery", false, "Enable Eitr refinery");
        config_custom_instance = Config.Bind<string>("Custom", "CustomPrefabs", "", "A comma-separated list of prefab names");

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
      switch (instanceName)
      {
        case "fire_pit(Clone)":
          EternalFuel = config_fire_pit.Value;
          break;

        case "bonfire(Clone)":
          EternalFuel = config_bonfire.Value;
          break;

        case "hearth(Clone)":
          EternalFuel = config_hearth.Value;
          break;

        case "piece_walltorch(Clone)":
          EternalFuel = config_piece_walltorch.Value;
          break;

        case "piece_groundtorch(Clone)":
          EternalFuel = config_piece_groundtorch.Value;
          break;

        case "piece_groundtorch_wood(Clone)":
          EternalFuel = config_piece_groundtorch_wood.Value;
          break;

        case "piece_groundtorch_green(Clone)":
          EternalFuel = config_piece_groundtorch_green.Value;
          break;

        case "piece_groundtorch_blue(Clone)":
          EternalFuel = config_piece_groundtorch_blue.Value;
          break;

        case "piece_brazierfloor01(Clone)":
          EternalFuel = config_piece_brazierfloor01.Value;
          break;

        case "piece_brazierceiling01(Clone)":
          EternalFuel = config_piece_brazierceiling01.Value;
          break;

        case "piece_jackoturnip(Clone)":
          EternalFuel = config_piece_jackoturnip.Value;
          break;

        case "piece_oven(Clone)":
          EternalFuel = config_piece_oven.Value;
          break;

        case "smelter(Clone)":
          EternalFuel = config_smelter.Value;
          break;

        case "blastfurnace(Clone)":
          EternalFuel = config_blastfurnace.Value;
          break;

        case "eitrrefinery(Clone)":
          EternalFuel = config_eitrrefinery.Value;
          break;

        case "piece_bathtub(Clone)":
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
