using Content.Server._SCP.Scps.peanut;
using Content.Shared.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._SCP.Scps.peanut;

public sealed class SharedPeanutSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ScpPeanutComponent, UpdateCanMoveEvent>(OnCanMove);
    }

    public void OnCanMove(EntityUid id, ScpPeanutComponent comp, UpdateCanMoveEvent args)
    {
        if (comp.canMove)
            return;

        args.Cancel();
    }
}
