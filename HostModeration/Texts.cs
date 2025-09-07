using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Marioalexsan.HostModeration;
internal static class Texts
{
    private const string Mod = "[HM]";

    // Errors and validation

    public static string CommandForbidden()
        => $"{Mod} You are not allowed to use that command";

    public static string ExpectedConnId()
        => $"{Mod} Expected a connection ID.";

    public static string ExpectedSteamID()
        => $"{Mod} Expected a valid Steam ID.";

    public static string ExpectedListIndex()
        => $"{Mod} Expected a valid list index.";

    public static string ExpectedNickname()
        => $"{Mod} Expected a nickname to match.";

    public static string ExpectedPageNumber()
        => $"{Mod} Page parameter should be a valid number";

    public static string ExpectedPageSize()
        => $"{Mod} Page size should be a valid number";

    // General

    public static string PageHeader(string subject, int page, int totalPages)
        => $"{Mod} {subject} (page {page}/{totalPages}):";

    public static string PageEntry(int index, string entry)
        => $" #{index} - {entry}";

    public static string PageNoData()
        => $" - [No data available]";

    public static string NoReasonSpecified()
        => $"[No reason specified]";

    public static string OperatorDetails(OperatorDetails peer)
        => $"{peer.CharacterNickname} [SteamID {peer.SteamId}]";

    public static string BannedClientDetails(BannedClientParameter peer)
        => $"{peer._characterNickname} [SteamID {peer._steamID}]";

    public static string ClientDetails(HC_PeerListEntry peer)
    {
        var connId = peer._dataID;
        var name = peer._peerPlayer.Network_nickname;
        var steamId = peer._peerPlayer._steamID;

        return $"{name} [ConnID {connId} SteamID {steamId}]";
    }

    public static string PromotedToOp()
        => $"{Mod} You have been promoted to operator";

    public static string DemotedFromOp()
        => $"{Mod} You have been demoted from operator";

    public static string PromotedTargetToOp()
        => $"{Mod} Promoted client to operator.";

    public static string DemotedTargetFromOp()
        => $"{Mod} Demoted client from operator.";

    public static string CannotPromoteDemoteHost()
        => $"{Mod} Hosts are always operators and cannot be promoted or demoted.";

    public static string CannotModerateHosts()
        => $"{Mod} Hosts cannot be moderated.";

    public static string OperatorsCannotModerateOperators()
        => $"{Mod} Operators can only be moderated by the host";

    public static string CannotPromoteBannedUsers()
        => $"{Mod} Client is banned, unban them first before trying to promote them.";

    public static string AlreadyBanned()
        => $"{Mod} Client is already banned.";

    public static string AlreadyAnOperator()
        => $"{Mod} Client is already an operator.";

    public static string NotBanned()
        => $"{Mod} Client was not found in the ban list.";

    public static string NotAnOperator()
        => $"{Mod} Client is not an operator.";

    public static string SteamIdNotConnected()
        => $"{Mod} That steam user is not connected to the server.";

    public static string InvalidCallerID()
        => $"{Mod} Invalid caller connection ID. You should notify the mod developer about this.";

    public static string YouHaveBeenWarned(bool warnedAnOperator)
        => $"{Mod} You have been warned by {(warnedAnOperator ? "the host" : "an operator")}";

    public static string WarnReason(string message)
        => $" Warn reason: {message}";

    public static string NoPlayerMatch()
        => $"{Mod} No player match was found for the given query.";

    public static string MultiplePlayerMatches()
        => $"{Mod} Multiple similar player matches were found. Try using a more specific search query.";

    public static string PlayerMatch(HC_PeerListEntry peer)
        => $"{Mod} Best player match is {ClientDetails(peer)}";

    // Notices - usually sent on operator actions within the online group

    public static string DemotedAndBannedUser(string caller, string target)
        => $"{Mod} {target} was demoted from operator and banned by {caller}.";

    public static string BannedUser(string caller, string target)
        => $"{Mod} {target} was banned by {caller}.";

    public static string UnbannedUser(string caller, string target)
        => $"{Mod} Client was unbanned.";

    public static string WarnedUser(string caller, string target)
        => $"{Mod} {target} was warned by {caller}";

    public static string KickedUser(string caller, string target)
        => $"{Mod} {target} was kicked by {caller}.";

    // Utils

    public static string ErrorText(string message)
        => $"<color=red>{message}</color>";

    public static string NoticeText(string message)
        => $"<color=yellow>{message}</color>";

    public static string SuccessText(string message)
        => $"<color=green>{message}</color>";

    public static string StatusText(bool success, string message)
        => success ? SuccessText(message) : ErrorText(message);

}
