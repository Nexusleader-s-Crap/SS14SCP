using Content.Shared.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._SCP.Actions;
public sealed class SharedActionFetchSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public bool GetAction<T>(EntityUid uid, [NotNullWhen(true)] out T? comp, out EntityUid id) where T : IComponent
    {
        foreach (var item in _actions.GetActions(uid))
        {
            if (TryComp<T>(item.Id, out var traverse))
            {
                id = item.Id;
                comp = traverse;
                return true;
            }
        }
        comp = default;
        id = uid;
        return false;
    }
}
