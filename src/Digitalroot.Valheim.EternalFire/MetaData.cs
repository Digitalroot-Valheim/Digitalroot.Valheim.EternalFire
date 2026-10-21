using Jotunn.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Digitalroot.Valheim.EternalFire
{
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public partial class Main
  {
    public const string Version = "0.0.1";
    public const string Name = "Eternal Fire";
    public const string Guid = "digitalroot.mods.eternalfire";
    public const string Namespace = $"{nameof(Digitalroot)}.{nameof(Valheim)}.{nameof(EternalFire)}";

    internal static class PluginConfigSection
    {
      internal const string General = nameof(General);
      internal const string Advanced = nameof(Advanced);
      internal const string Fireplaces = nameof(Fireplaces);
      internal const string CookingStations = nameof(CookingStations);
      internal const string Smelters = nameof(Smelters);
      internal const string ShieldGenerators = nameof(ShieldGenerators);
      internal const string Custom = nameof(Custom);
      internal const string BoneAppetit = nameof(BoneAppetit);
      internal const string IronFirePit = nameof(IronFirePit);
      internal const string SnowLantern = nameof(SnowLantern);
      internal const string CampFire = nameof(CampFire);
      internal const string Sconce = nameof(Sconce);
      internal const string StandingIronTorch = nameof(StandingIronTorch);
      internal const string StandingWoodTorch = nameof(StandingWoodTorch);
      internal const string StandingGreenBurningIronTorch = nameof(StandingGreenBurningIronTorch);
      internal const string StandingBlueBurningIronTorch = nameof(StandingBlueBurningIronTorch);
      internal const string StandingBrazier = nameof(StandingBrazier);
      internal const string StandingBlueBrazier = nameof(StandingBlueBrazier);
      internal const string HangingBrazier = nameof(HangingBrazier);
      internal const string JackOTurnip = nameof(JackOTurnip);
      internal const string HotTub = nameof(HotTub);
      internal const string StoneOven = nameof(StoneOven);
      internal const string BlastFurnace = nameof(BlastFurnace);
      internal const string EitrRefinery = nameof(EitrRefinery);
      internal const string ShieldGenerator = nameof(ShieldGenerator);
      internal const string CustomPrefabs = nameof(CustomPrefabs);
      internal const string SmokelessHearth = "rk_hearth";
      internal const string SmokelessHearthName = "Smokeless Hearth";
      internal const string SmokelessFirePit = "rk_campfire";
      internal const string SmokelessFirePitName = "Smokeless Campfire";
      internal const string SmokelessHangingBrazier = "rk_brazier";
      internal const string SmokelessHangingBrazierName = "Smokeless Hanging Brazier";
      internal const string BoneAppetitGuid = "com.rockerkitten.boneappetit";
      internal const string VPCGuid = "MidnightsFX.ValheimCommunityPatch";
    }
  }
}
