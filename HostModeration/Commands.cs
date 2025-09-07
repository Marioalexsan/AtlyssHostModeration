using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marioalexsan.HostModeration;

public delegate bool CommandAction(int callerConnectionId, string[] parts, Action<string> sendMessage);
public delegate bool WrappedSteamIdCommand(int callerConnectionId, string steamId, out string statusMessage);
public delegate bool WrappedSteamIdParamsCommand(int callerConnectionId, string steamId, string[] parts, out string statusMessage);

internal class CommandData(string name, string description, CommandAction action)
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public bool RequiresOperator { get; set; } = true;
    public bool HostOnly { get; set; } = false;
    public CommandAction Action { get; set; } = action;
}

internal static class Commands
{
    public static readonly CommandData[] DataList;
    public static readonly Dictionary<string, CommandData> Data;

    static Commands()
    {
        DataList =
        [
            // Utils
            new CommandData("hm-help", "show commands", HmHelp),
            new CommandData("hm-clients", "show clients", HmClients),
            new CommandData("hm-operators", "show operators", HmOperators),
            new CommandData("hm-bans", "show banned clients", HmBanned),
            new CommandData("hm-find-name", "find a client by name", HmFindClient),

            // Operator assignment
            MakeNicknameCommand("hm-op", "promote to operator", HostModeration.PromoteToOperatorBySteamId, hostOnly: true),
            MakeSteamIdCommand("hm-op-steam", "promote to operator", HostModeration.PromoteToOperatorBySteamId, hostOnly: true),
            MakeConnIdCommand("hm-op-conn", "promote to operator", HostModeration.PromoteToOperatorBySteamId, hostOnly: true),

            MakeNicknameCommand("hm-deop", "demote operator", HostModeration.DemoteFromOperatorBySteamId, hostOnly: true),
            MakeSteamIdCommand("hm-deop-steam", "demote operator", HostModeration.DemoteFromOperatorBySteamId, hostOnly: true),
            MakeConnIdCommand("hm-deop-conn", "demote operator", HostModeration.DemoteFromOperatorBySteamId, hostOnly: true),

            // Moderation
            MakeNicknameCommand("hm-ban", "ban client", HostModeration.BanUserBySteamId),
            MakeSteamIdCommand("hm-ban-steam", "ban client", HostModeration.BanUserBySteamId),
            MakeConnIdCommand("hm-ban-conn", "ban client", HostModeration.BanUserBySteamId),

            MakeSteamIdCommand("hm-unban-steam", "unban client", HostModeration.UnbanUserBySteamId),
            new CommandData("hm-unban-id", "unban client by ban ID", HmUnban),

            MakeNicknameCommand("hm-kick", "kick client", HostModeration.KickUserBySteamId),
            MakeSteamIdCommand("hm-kick-steam", "kick client", HostModeration.KickUserBySteamId),
            MakeConnIdCommand("hm-kick-conn", "kick client", HostModeration.KickUserBySteamId),

            MakeSteamIdCommand("hm-warn-steam", "warn client", HmWarn),
            MakeConnIdCommand("hm-warn-conn", "warn client", HmWarn),
        ];

        Data = DataList.ToDictionary(x => x.Name);
    }

    private static bool HmFindClient(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        if (parts.Length < 2)
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedNickname()));
            return false;
        }

        var steamId = HostModeration.FindSteamIdBasedOnMatches(parts[1..], out var statusMessage);

        sendMessage(Texts.StatusText(steamId != null, statusMessage));
        return steamId != null;
    }

    private static bool HmWarn(int callerConnectionId, string steamId, string[] parts, out string statusMessage)
    {
        var message = string.Join(' ', parts[2..]);

        if (string.IsNullOrWhiteSpace(message))
            message = Texts.NoReasonSpecified();

        return HostModeration.WarnUserBySteamId(callerConnectionId, steamId, message, out statusMessage);
    }

    private static bool HmUnban(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], out var banListIndex))
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedListIndex()));
            return false;
        }

        bool success = HostModeration.UnbanUserByBanListIndex(callerConnectionId, banListIndex, out var statusMessage);

        sendMessage(Texts.StatusText(success, statusMessage));
        return success;
    }

    private static bool HmHelp(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        static string[] GetCommands()
        {
            return [.. DataList.Select(x => $"{x.Name} - {x.Description}")];
        }

        return ShowPaginatedList(parts, "Commands", GetCommands, sendMessage);
    }

    private static bool HmClients(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        static string[] GetClients()
        {
            var peers = HostConsole._current._peerListEntries;
            var clients = new string[peers.Count];

            for (int i = 0; i < peers.Count; i++)
                clients[i] = Texts.ClientDetails(peers[i]);

            return clients;
        }

        return ShowPaginatedList(parts, "Clients", GetClients, sendMessage);
    }

    private static bool HmOperators(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        static string[] GetOperators()
        {
            var peers = HostModeration.Data.SteamIdOperators;
            var clients = new string[peers.Count];

            for (int i = 0; i < peers.Count; i++)
                clients[i] = Texts.OperatorDetails(peers[i]);

            return clients;
        }

        return ShowPaginatedList(parts, "Operators", GetOperators, sendMessage);
    }

    private static bool HmBanned(int callerConnectionId, string[] parts, Action<string> sendMessage)
    {
        static string[] GetBannedClients()
        {
            var peers = AtlyssNetworkManager._current._bannedClientList;
            var clients = new string[peers.Count];

            for (int i = 0; i < peers.Count; i++)
                clients[i] = Texts.BannedClientDetails(peers[i]);

            return clients;
        }

        return ShowPaginatedList(parts, "Banned clients", GetBannedClients, sendMessage);
    }

    private static CommandData MakeSteamIdCommand(string name, string description, WrappedSteamIdCommand executeCommand, bool hostOnly = false, bool requiresOperator = true)
        => MakeSteamIdCommand(name, description, (int callerConnectionId, string steamId, string[] parts, out string statusMessage) => executeCommand(callerConnectionId, steamId, out statusMessage), hostOnly, requiresOperator);

    private static CommandData MakeSteamIdCommand(string name, string description, WrappedSteamIdParamsCommand executeCommand, bool hostOnly = false, bool requiresOperator = true)
    {
        return new CommandData(name, $"{description} by SteamID", (callerConnectionId, parts, sendMessage) =>
        {
            return SteamIdCommand(callerConnectionId, parts, sendMessage, executeCommand);
        })
        {
            HostOnly = hostOnly,
            RequiresOperator = requiresOperator
        };
    }

    private static CommandData MakeConnIdCommand(string name, string description, WrappedSteamIdCommand executeCommand, bool hostOnly = false, bool requiresOperator = true)
        => MakeConnIdCommand(name, description, (int callerConnectionId, string steamId, string[] parts, out string statusMessage) => executeCommand(callerConnectionId, steamId, out statusMessage), hostOnly, requiresOperator);

    private static CommandData MakeConnIdCommand(string name, string description, WrappedSteamIdParamsCommand executeCommand, bool hostOnly = false, bool requiresOperator = true)
    {
        return new CommandData(name, $"{description} by ConnID", (callerConnectionId, parts, sendMessage) =>
        {
            return ConnIdCommand(callerConnectionId, parts, sendMessage, executeCommand);
        })
        {
            HostOnly = hostOnly,
            RequiresOperator = requiresOperator
        };
    }

    private static CommandData MakeNicknameCommand(string name, string description, WrappedSteamIdCommand executeCommand, bool hostOnly = false, bool requiresOperator = true)
        => MakeNicknameCommand(name, description, (int callerConnectionId, string steamId, string[] parts, out string statusMessage) => executeCommand(callerConnectionId, steamId, out statusMessage), hostOnly, requiresOperator);

    private static CommandData MakeNicknameCommand(string name, string description, WrappedSteamIdParamsCommand executeCommand, bool hostOnly = false, bool requiresOperator = true)
    {
        return new CommandData(name, $"{description} by name", (callerConnectionId, parts, sendMessage) =>
        {
            return NicknameCommand(callerConnectionId, parts, sendMessage, executeCommand);
        })
        {
            HostOnly = hostOnly,
            RequiresOperator = requiresOperator
        };
    }

    private static bool ConnIdCommand(int callerConnectionId, string[] parts, Action<string> sendMessage, WrappedSteamIdParamsCommand executeCommand)
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], out var result) || !ConnUtils.ConnectionIdToSteamId(result, out var steamId))
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedConnId()));
            return false;
        }

        bool success = executeCommand(callerConnectionId, steamId, parts, out var statusMessage);

        sendMessage(Texts.StatusText(success, statusMessage));
        return success;
    }

    private static bool SteamIdCommand(int callerConnectionId, string[] parts, Action<string> sendMessage, WrappedSteamIdParamsCommand executeCommand)
    {
        if (parts.Length < 2 || !Utils.IsValidSteamId(parts[1]))
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedSteamID()));
            return false;
        }

        bool success = executeCommand(callerConnectionId, parts[1], parts, out var statusMessage);

        sendMessage(Texts.StatusText(success, statusMessage));
        return success;
    }

    private static bool NicknameCommand(int callerConnectionId, string[] parts, Action<string> sendMessage, WrappedSteamIdParamsCommand executeCommand)
    {
        if (parts.Length < 2)
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedNickname()));
            return false;
        }

        var steamId = HostModeration.FindSteamIdBasedOnMatches(parts[1..], out var statusMessage);

        if (steamId == null)
        {
            sendMessage(Texts.ErrorText(statusMessage));
            return false;
        }

        bool success = executeCommand(callerConnectionId, steamId, parts, out statusMessage);

        sendMessage(Texts.StatusText(success, statusMessage));
        return success;
    }

    private static bool ShowPaginatedList(string[] parts, string subject, Func<string[]> retrieveData, Action<string> sendMessage)
    {
        var page = 1;
        var pageSize = 7;

        if (parts.Length >= 2 && !int.TryParse(parts[1], out page))
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedPageNumber()));
            return false;
        }

        if (parts.Length >= 3 && !(int.TryParse(parts[2], out pageSize) && 5 <= pageSize && pageSize <= 100))
        {
            sendMessage(Texts.ErrorText(Texts.ExpectedPageSize()));
            return false;
        }

        var entries = retrieveData();

        var totalPages = (int)Math.Ceiling((float)entries.Length / pageSize);
        var firstEntry = Math.Clamp((page - 1) * pageSize, 0, entries.Length);
        var lastEntry = Math.Clamp(page * pageSize, 0, entries.Length);

        sendMessage(Texts.PageHeader(subject, page, totalPages));

        for (int i = firstEntry; i < lastEntry; i++)
            sendMessage(Texts.PageEntry(i, entries[i]));

        if (firstEntry == lastEntry)
            sendMessage(Texts.PageNoData());

        return true;
    }
}
