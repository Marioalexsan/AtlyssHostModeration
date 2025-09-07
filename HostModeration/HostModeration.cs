using BepInEx;
using HarmonyLib;
using Mirror;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UI;

namespace Marioalexsan.HostModeration;

[BepInPlugin(ModInfo.GUID, ModInfo.NAME, ModInfo.VERSION)]
public class HostModeration : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony($"{ModInfo.GUID}");

    public static HostModerationData Data { get; private set; } = new();

    public static string DataPath => Path.Combine(Paths.ConfigPath, $"{ModInfo.GUID}_Data.json");

    public const int HostConsoleFakeConnectionId = -1337; // Symbolizes the server host console

    public static bool ProcessCommand(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        if (!Commands.Data.TryGetValue(parts[0], out var data))
            return true;

        if (data.RequiresOperator && !ConnUtils.HasOperatorRole(callerConnectionId))
        {
            sendMessage(Texts.ErrorText(Texts.CommandForbidden()));
            return false;
        }

        if (data.HostOnly && !ConnUtils.IsHost(callerConnectionId))
        {
            sendMessage(Texts.ErrorText(Texts.CommandForbidden()));
            return false;
        }

        data.Action(callerConnectionId, parts, sendMessage);
        return false;
    }

    public void Awake()
    {
        _harmony.PatchAll();
        LoadData();
        UnityEngine.Debug.Log($"HostModeration loaded.");
    }

    public static void LoadData()
    {
        if (!File.Exists(DataPath))
        {
            Data = new();
            return;
        }

        Data = JsonConvert.DeserializeObject<HostModerationData>(File.ReadAllText(DataPath)) ?? throw new InvalidOperationException("JSON deserialization failed");
    }

    public static void SaveData()
    {
        File.WriteAllText(DataPath, JsonConvert.SerializeObject(Data));
    }

    public static bool PromoteToOperatorBySteamId(int callerConnectionId, string steamId, out string statusMessage)
    {
        if (ConnUtils.SteamIdToPeer(steamId, out var peer) && ConnUtils.IsHost(peer._dataID))
        {
            statusMessage = Texts.CannotPromoteDemoteHost();
            return false;
        }

        var bannedUsers = AtlyssNetworkManager._current._bannedClientList;

        for (int i = 0; i < bannedUsers.Count; i++)
        {
            if (bannedUsers[i]._steamID == steamId)
            {
                statusMessage = Texts.CannotPromoteBannedUsers();
                return false;
            }
        }

        for (int i = 0; i < Data.SteamIdOperators.Count; i++)
        {
            if (Data.SteamIdOperators[i].SteamId == steamId)
            {
                statusMessage = Texts.AlreadyAnOperator();
                return false;
            }
        }

        var connectedUsers = HostConsole._current._peerListEntries;

        var characterNickname = ConnUtils.SteamIdToPeer(steamId, out var targetPeer) ? targetPeer._peerPlayer._nickname : "Unknown";

        Data.SteamIdOperators.Add(new OperatorDetails()
        {
            SteamId = steamId,
            CharacterNickname = characterNickname
        });
        SaveData();

        TrySendChatMessageToSteamId(steamId, Texts.NoticeText(Texts.PromotedToOp()));

        statusMessage = Texts.PromotedTargetToOp();
        return true;
    }

    public static bool DemoteFromOperatorBySteamId(int callerConnectionId, string steamId, out string statusMessage)
    {
        if (ConnUtils.SteamIdToConnectionId(steamId, out int connId) && ConnUtils.IsHost(connId))
        {
            statusMessage = Texts.CannotPromoteDemoteHost();
            return false;
        }

        for (int i = 0; i < Data.SteamIdOperators.Count; i++)
        {
            if (Data.SteamIdOperators[i].SteamId == steamId)
            {
                Data.SteamIdOperators.RemoveAt(i);
                SaveData();

                TrySendChatMessageToSteamId(steamId, Texts.NoticeText(Texts.DemotedFromOp()));

                statusMessage = Texts.DemotedTargetFromOp();
                return true;
            }
        }

        statusMessage = Texts.NotAnOperator();
        return false;
    }

    public static bool BanUserBySteamId(int callerConnectionId, string steamId, out string statusMessage)
    {
        var callerName = ConnUtils.ConnectionIdToName(callerConnectionId);
        var targetName = $"Unknown [SteamID {steamId}]";
        var characterNickname = "Unknown";

        if (ConnUtils.SteamIdToPeer(steamId, out var targetPeer))
        {
            if (ConnUtils.IsHost(targetPeer._dataID))
            {
                statusMessage = Texts.CannotModerateHosts();
                return false;
            }

            if (ConnUtils.HasOperatorRole(targetPeer._dataID) && !ConnUtils.IsHost(callerConnectionId))
            {
                statusMessage = Texts.OperatorsCannotModerateOperators();
                return false;
            }

            targetName = Texts.ClientDetails(targetPeer);
            characterNickname = targetPeer._peerPlayer._nickname;

            HostConsole._current._cmdManager.Init_KickClient(HostConsole._current._peerListEntries.IndexOf(targetPeer), targetPeer._dataID);
        }


        var bannedUsers = AtlyssNetworkManager._current._bannedClientList;

        for (int i = 0; i < bannedUsers.Count; i++)
        {
            if (bannedUsers[i]._steamID == steamId)
            {
                statusMessage = Texts.AlreadyBanned();
                return false;
            }
        }

        bannedUsers.Add(new BannedClientParameter()
        {
            _address = "",
            _characterNickname = characterNickname,
            _steamID = steamId
        });

        var demoted = DemoteFromOperatorBySteamId(callerConnectionId, steamId, out _);

        ProfileDataManager._current.Save_HostSettingsData();

        NotifyOperators(demoted ? Texts.DemotedAndBannedUser(callerName, targetName) : Texts.BannedUser(callerName, targetName));

        statusMessage = "";
        return true;
    }

    public static bool UnbanUserByBanListIndex(int callerConnectionId, int banListIndex, out string statusMessage)
    {
        var bannedUsers = AtlyssNetworkManager._current._bannedClientList;

        if (!(0 <= banListIndex && banListIndex < bannedUsers.Count))
        {
            statusMessage = Texts.ExpectedListIndex();
            return false;
        }

        return UnbanUserBySteamId(callerConnectionId, bannedUsers[banListIndex]._steamID, out statusMessage);
    }

    public static bool UnbanUserBySteamId(int callerConnectionId, string steamId, out string statusMessage)
    {
        var bannedUsers = AtlyssNetworkManager._current._bannedClientList;

        BannedClientParameter? user = null;

        for (int i = 0; i < bannedUsers.Count; i++)
        {
            if (bannedUsers[i]._steamID == steamId)
            {
                user = bannedUsers[i];
                bannedUsers.RemoveAt(i--);
                continue;
            }
        }


        if (user != null)
        {
            var callerName = ConnUtils.ConnectionIdToName(callerConnectionId);
            var targetName = Texts.BannedClientDetails(user);

            ProfileDataManager._current.Save_HostSettingsData();
            NotifyOperators(Texts.UnbannedUser(callerName, targetName));

            statusMessage = "";
            return true;
        }
        else
        {
            statusMessage = Texts.NotBanned();
            return false;
        }
    }

    public static bool WarnUserBySteamId(int callerConnectionId, string steamId, string message, out string statusMessage)
    {
        if (!ConnUtils.SteamIdToPeer(steamId, out var targetPeer))
        {
            statusMessage = Texts.SteamIdNotConnected();
            return false;
        }

        if (ConnUtils.IsHost(targetPeer._dataID))
        {
            statusMessage = Texts.CannotModerateHosts();
            return false;
        }

        if (ConnUtils.HasOperatorRole(targetPeer._dataID))
        {
            if (!ConnUtils.ConnectionIdToPeer(callerConnectionId, out var peer) || !ConnUtils.IsHost(callerConnectionId))
            {
                statusMessage = Texts.OperatorsCannotModerateOperators();
                return false;
            }
        }

        var connectedUsers = HostConsole._current._peerListEntries;

        TrySendChatMessageToConnectionId(targetPeer._dataID, Texts.NoticeText(Texts.YouHaveBeenWarned(ConnUtils.HasOperatorRole(targetPeer._dataID))));
        TrySendChatMessageToConnectionId(targetPeer._dataID, Texts.NoticeText(Texts.WarnReason(message)));

        var callerName = ConnUtils.ConnectionIdToName(callerConnectionId);
        var targetName = ConnUtils.SteamIdToName(steamId);

        NotifyOperators(Texts.WarnedUser(callerName, targetName), targetPeer._dataID);
        NotifyOperators(Texts.WarnReason(message), targetPeer._dataID);

        statusMessage = "";
        return true;
    }

    public static bool KickUserBySteamId(int callerConnectionId, string steamId, out string statusMessage)
    {
        if (!ConnUtils.SteamIdToPeer(steamId, out var peer))
        {
            statusMessage = Texts.SteamIdNotConnected();
            return false;
        }

        if (ConnUtils.IsHost(peer._dataID))
        {
            statusMessage = Texts.CannotModerateHosts();
            return false;
        }

        if (ConnUtils.HasOperatorRole(peer._dataID) && !ConnUtils.IsHost(callerConnectionId))
        {
            statusMessage = Texts.OperatorsCannotModerateOperators();
            return false;
        }

        var callerName = ConnUtils.ConnectionIdToName(callerConnectionId);
        var targetName = ConnUtils.SteamIdToName(steamId);

        HostConsole._current._cmdManager.Init_KickClient(HostConsole._current._peerListEntries.IndexOf(peer), peer._dataID);

        NotifyOperators(Texts.KickedUser(callerName, targetName));

        statusMessage = "";
        return true;
    }

    public static string? FindSteamIdBasedOnMatches(string[] matches, out string statusMessage)
    {
        int maxScore = 0;
        HC_PeerListEntry? bestPeer = null;
        bool multipleMatches = false;

        var peers = HostConsole._current._peerListEntries;

        for (int i = 0; i < peers.Count; i++)
        {
            var displayName = peers[i]._peerPlayer._nickname;
            var score = 0;

            // Skip position 0 since it's the command name
            for (int k = 0; k < matches.Length; k++)
            {
                if (displayName.Contains(matches[k], StringComparison.InvariantCultureIgnoreCase))
                    score += matches[k].Length;
            }

            if (score > maxScore)
            {
                maxScore = score;
                bestPeer = peers[i];
                multipleMatches = false;
            }
            else if (score == maxScore)
            {
                multipleMatches = true;
            }
        }

        if (bestPeer == null)
        {
            statusMessage = Texts.NoPlayerMatch();
            return null;
        }

        if (multipleMatches)
        {
            statusMessage = Texts.MultiplePlayerMatches();
            return null;
        }

        statusMessage = Texts.PlayerMatch(bestPeer);
        return bestPeer._peerPlayer._steamID;
    }

    public static bool TrySendChatMessageToSteamId(string steamId, string message)
    {
        if (ConnUtils.SteamIdToPeer(steamId, out var peer))
        {
            peer._peerPlayer._chatBehaviour.Target_RecieveMessage(message);
            return true;
        }

        return false;
    }

    public static bool TrySendChatMessageToConnectionId(int connectionId, string message)
    {
        if (ConnUtils.ConnectionIdToPeer(connectionId, out var peer))
        {
            peer._peerPlayer._chatBehaviour.Target_RecieveMessage(message);
            return true;
        }

        return false;
    }

    public static void NotifyOperators(string message, int operatorToExclude = -1)
    {
        var connectedUsers = HostConsole._current._peerListEntries;

        for (int i = 0; i < connectedUsers.Count; i++)
        {
            var opConnectionId = connectedUsers[i]._dataID;

            if (ConnUtils.HasOperatorRole(opConnectionId) && opConnectionId != operatorToExclude)
                TrySendChatMessageToConnectionId(opConnectionId, Texts.NoticeText(message));
        }

        HostConsole._current.New_LogMessage(Texts.NoticeText(message));
    }
}