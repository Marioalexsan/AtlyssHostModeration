using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Marioalexsan.HostModeration;
internal static class ConnUtils
{
    public static bool HasOperatorRole(int connectionId)
    {
        if (IsHost(connectionId))
            return true;

        if (!ConnectionIdToSteamId(connectionId, out var steamId))
            return false;

        return HostModeration.Data.SteamIdOperators.FindIndex(x => x.SteamId == steamId) != -1;
    }

    public static bool IsHost(int connectionId)
    {
        if (connectionId == HostModeration.HostConsoleFakeConnectionId)
            return true;

        var connectedUsers = HostConsole._current._peerListEntries;

        for (int i = 0; i < connectedUsers.Count; i++)
        {
            if (connectedUsers[i]._dataID == connectionId)
                return connectedUsers[i]._peerPlayer._isHostPlayer;
        }

        return false;
    }

    public static bool ConnectionIdToSteamId(int connectionId, [NotNullWhen(true)] out string? steamId)
    {
        var connectedUsers = HostConsole._current._peerListEntries;

        for (int i = 0; i < connectedUsers.Count; i++)
        {
            if (connectedUsers[i]._dataID == connectionId)
            {
                steamId = connectedUsers[i]._peerPlayer._steamID;
                return true;
            }
        }

        steamId = null;
        return false;
    }

    public static bool SteamIdToConnectionId(string steamId, out int connectionId)
    {
        var connectedUsers = HostConsole._current._peerListEntries;

        for (int i = 0; i < connectedUsers.Count; i++)
        {
            if (connectedUsers[i]._peerPlayer._steamID == steamId)
            {
                connectionId = connectedUsers[i]._dataID;
                return true;
            }
        }

        connectionId = -1;
        return false;
    }

    public static bool ConnectionIdToPeer(int connectionId, [NotNullWhen(true)] out HC_PeerListEntry? peer)
    {
        var connectedUsers = HostConsole._current._peerListEntries;

        for (int i = 0; i < connectedUsers.Count; i++)
        {
            if (connectedUsers[i]._dataID == connectionId)
            {
                peer = connectedUsers[i];
                return true;
            }
        }

        peer = null;
        return false;
    }

    public static string SteamIdToName(string steamId)
    {
        if (SteamIdToPeer(steamId, out var peer))
            return Texts.ClientDetails(peer);

        return "Unknown";
    }

    public static string ConnectionIdToName(int connectionId)
    {
        if (connectionId == HostModeration.HostConsoleFakeConnectionId)
            return "Host Console";

        if (ConnectionIdToPeer(connectionId, out var peer))
            return Texts.ClientDetails(peer);

        return "Unknown";
    }

    public static bool SteamIdToPeer(string steamId, [NotNullWhen(true)] out HC_PeerListEntry? peer)
    {
        var connectedUsers = HostConsole._current._peerListEntries;

        for (int i = 0; i < connectedUsers.Count; i++)
        {
            if (connectedUsers[i]._peerPlayer._steamID == steamId)
            {
                peer = connectedUsers[i];
                return true;
            }
        }

        peer = null;
        return false;
    }
}
