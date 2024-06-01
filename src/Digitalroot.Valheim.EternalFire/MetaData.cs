using System.Diagnostics.CodeAnalysis;

namespace Digitalroot.Valheim.EternalFire
{
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public partial class Main
  {
    public const string Version = "0.0.1";
    public const string Name = "Eternal Fire";
    public const string Guid = "digitalroot.mods.eternalfire";
    public const string Namespace = "Digitalroot.Valheim" + nameof(EternalFire);

    internal static class PluginConfigSection
    {
      internal static string General = nameof(General);
      internal static string Fireplaces = nameof(Fireplaces);
      internal static string CookingStations = nameof(CookingStations);
      internal static string Smelters = nameof(Smelters);
      internal static string Custom = nameof(Custom);
    }
  }
}
