# Host Moderation

Fully server-side mod that enhances server moderation:
- Adds additional chat commands for facilitating moderation duties
- Adds SteamID, ConnID and name variants for moderation commands
- Allows hosts to assign operator roles to trusted users; operators can then moderate the server on their own

# Operators

Operators are users that are allowed to perform moderation on the host's behalf.

As a rule, hosts are also operators, and cannot be promoted or demoted.

The commands included in this mod can only be used by operators. Additionally, moderation commands targeting operators can only be performed by the host.

# Command parameters

Some commands accept either a Steam ID, connection ID or player name to match.

The connection ID is an unique identifier assigned to each connected client, and is unique per connection within a session. This is also used by vanilla Atlyss for identifying clients.

Each client also has a Steam ID assigned to it. Steam IDs are unique per player, and commands that accept Steam IDs can be used even when the target player is not connected.

Some commands also accept player name queries. In that case, the closest matching player will be selected by the command. In the case where there are multiple close matches found, you will have to use a more accurate query, or use the Steam ID or connection ID variant of that command.

# Host Console

The commands are usable from the vanilla Host Console, however commands that take more than one parameter won't work correctly due to vanilla game quirks.

Prefer using the chat commands if possible.

# Utility commands

Paginated list commands accept an optional page number parameter and a page size parameter.

If not specified, page number defaults to 1, and page size defaults to 7.

Page size can be between 5 and 100 elements.

`/hm-help [page number] [page size]`

Displays a paginated list of available commands.

`/hm-clients [page number] [page size]`

Displays a paginated list of currently connected clients (nickname, Steam ID and connection ID).

`/hm-operators [page number] [page size]`

Displays a paginated list of operators (nickname and Steam ID).

`/hm-bans [page number] [page size]`

Displays a paginated list of banned clients (nickname and Steam ID).

`/hm-find-name <partial player name...>`

Searches for a player based on the search query and returns the closest match.

Usage example:

```
Player list:
- Coboa the First
- Coboa the Second
- ALLCAPSCHANG

/hm-find-name Coboa
 => Coboa the First, ConnID 0, SteamID 12345

/hm-find-name Coboa the First
 => Coboa the First, ConnID 0, SteamID 12345

/hm-find-name Coboa the First and Best
 => [No player match]

/hm-find-name the Second
 => Coboa the Second, ConnID 3, SteamID 23456

/hm-find-name allcaps
 => ALLCAPSCHANG, ConnID 16, SteamID 12043

/hm-find-name WhoTheHellIsThisGuy
 => [No player match]
```

# Operator assignment commands

Only the host can promote and demote operators.

`/hm-op <player name...>`

`/hm-op-steam <Steam ID>`

`/hm-op-conn <connection ID>`

Promotes a client to operator by their connection ID, Steam ID or name.

*If a client is banned, they need to be unbanned before promoting them to operator.*

`/hm-deop <player name...>`

`/hm-deop-steam <Steam ID>`

`/hm-deop-conn <connection ID>`

Demotes a client from operator by their connection ID, Steam ID or name.

*You can use /hm-operators to see which clients are operators.*

# Moderation commands

Operators (including the host) will be notified whenever another operator takes action against a client.

`/hm-ban <partial player name...>`

`/hm-ban-steam <Steam ID>`

`/hm-ban-conn <connection ID>`

Bans a client from the server by their connection ID, Steam ID or name. If they were an operator, they will also be demoted.

`/hm-unban-steam <Steam ID>`

`/hm-unban-id <ban list index>`

Unbans a client from the server by their Steam ID, or by their ID (index) in the ban list.

*You can use /hm-bans to see which Steam IDs were banned, and their ban index.*

`/hm-kick <player name...>`

`/hm-kick-steam <Steam ID>`

`/hm-kick-conn <connection ID>`

Kicks a client from the server by their connection ID, Steam ID or name.

*Unlike bans, kicking will not automatically demote operators.*

`/hm-warn-steam <Steam ID> <warn reason...>`

`/hm-warn-conn <connection ID> <warn reason...>`

Sends a warning to a client by their connection ID or Steam ID.

The warn reason will be sent to the warned client, and operators will be notified of the warn reason.

*This command doesn't support specifying player names directly; use `/hm-find-name <player name...>` and then `/hm-warn <connection ID> <warn reason...>` as a workaround.*

# Mod Compatibility

This is a fully server-side mod that is also compatible with vanilla clients. You don't need to have the mod as a client to be able to use its commands on a server.

HostModeration targets the following game versions and mods:

- ATLYSS 82025.a2

Compatibility with other game versions and mods is not guaranteed, especially for updates with major changes.