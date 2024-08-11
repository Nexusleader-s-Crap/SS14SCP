using Content.Server._SCP.Scps.oldman.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Shared._SCP.Scps.Oldman;

public sealed class PocketDimensionSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<PocketDimensionSenderComponent, TogglePocketDimension>(OnTogglePocket);
    }

    public void OnTogglePocket(EntityUid owner, PocketDimensionSenderComponent pocket, TogglePocketDimension args)
    {
        if (args.Handled)
            return;
        if (pocket.traversing)
            return;
        pocket.traversing = true;
        _actions.SetCooldown(args.Action, TimeSpan.FromSeconds(5f));
        var ev = new TogglePocketDimensionDoAfter();
        var popup = "scp-oldman-traversepocket";

        if (!TryComp<TransformComponent>(args.Performer, out var transform))
            return;
        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, TimeSpan.FromSeconds(3f), ev, args.Performer)
        {
            BreakOnMove = true,
        };
        if (_doAfter.TryStartDoAfter(doAfterArgs))
            _popup.PopupCoordinates(Loc.GetString(popup), transform.Coordinates, PopupType.LargeCaution);
    }
}

