// SPDX-License-Identifier: MPL-2.0

using Content.Server.Administration;
using Content.Server.Mind;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._KS14.FieldCommand;

/// <summary>Creates a Field Commander at an administrator's position and transfers their mind to it.</summary>
[AdminCommand(AdminFlags.Admin)]
public sealed partial class PromoteFieldCommanderCommand : IConsoleCommand
{
    [Dependency] private IEntityManager _entities = default!;
    [Dependency] private IPlayerManager _players = default!;

    public string Command => "promotefieldcommander";
    public string Description => "Promotes yourself, or a selected player, to Field Commander.";
    public string Help => "Usage: promotefieldcommander [player]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError(Help);
            return;
        }

        var session = shell.Player;
        if (args.Length == 1)
        {
            if (!_players.TryGetSessionByUsername(args[0], out session))
            {
                shell.WriteError($"Player '{args[0]}' was not found.");
                return;
            }
        }

        if (session?.AttachedEntity is not { } oldBody)
        {
            shell.WriteError("A connected player with a body is required.");
            return;
        }

        var commander = _entities.SpawnEntity("KsFieldCommander", _entities.GetComponent<TransformComponent>(oldBody).Coordinates);
        _entities.System<MindSystem>().ControlMob(session.UserId, commander);
        shell.WriteLine($"Promoted {session.Name} to Field Commander.");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        => args.Length == 1
            ? CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "player")
            : CompletionResult.Empty;
}
