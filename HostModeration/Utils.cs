using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marioalexsan.HostModeration;

internal static class Utils
{
    public static bool IsValidSteamId(string steamId) => ulong.TryParse(steamId, out var result) && new CSteamID(result).IsValid();
}
