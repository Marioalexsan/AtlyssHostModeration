using HarmonyLib;
using Marioalexsan.HostModeration;

[HarmonyPatch(typeof(ChatBehaviour), nameof(ChatBehaviour.UserCode_Cmd_SendChatMessage__String__ChatChannel))]
static class ProcessOperatorCommands
{
    static bool Prefix(ChatBehaviour __instance, string _message, ChatBehaviour.ChatChannel _chatChannel)
    {
        if (_message.Contains('<') && _message.Contains('>') || _message.Contains('\0') || _message.Length > 125 || __instance._messageSendBuffer)
            return true;

        if (_message == "")
            return true;

        var command = _message.Trim();

        if (!command.StartsWith("/"))
            return true;

        var parts = command[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return true;

        return HostModeration.ProcessCommand(__instance.connectionToClient.connectionId, parts, __instance.Target_RecieveMessage);
    }
}

// Note: Host Console's implementation is complete junk, and I don't think it's good enough to be used for commands.

[HarmonyPatch(typeof(ConsoleCommandManager), nameof(ConsoleCommandManager.Init_ConsoleCommand))]
static class AddModerationCommandsToConsole
{
    static bool Prefix(string _name, string _output)
    {
        string[] parts = string.IsNullOrWhiteSpace(_output) ? [_name] : [_name, _output];

        return HostModeration.ProcessCommand(HostModeration.HostConsoleFakeConnectionId, parts, HostConsole._current.New_LogMessage);
    }
}