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
      internal static readonly string General = nameof(General);
      internal static readonly string Fireplaces = nameof(Fireplaces);
      internal static readonly string CookingStations = nameof(CookingStations);
      internal static readonly string Smelters = nameof(Smelters);
      internal static readonly string Custom = nameof(Custom);
      internal static readonly string BoneAppetit = nameof(BoneAppetit);
      internal const string SmokelessHearth = "rk_hearth";
      internal const string SmokelessHearthName = "Smokeless Hearth";
      internal const string SmokelessFirePit = "rk_campfire";
      internal const string SmokelessFirePitName = "Smokeless Campfire";
      internal const string SmokelessHangingBrazier = "rk_brazier";
      internal const string SmokelessHangingBrazierName = "Smokeless Hanging Brazier";
    }
  }
}
