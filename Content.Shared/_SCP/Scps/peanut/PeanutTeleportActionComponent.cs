using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._SCP.Scps.peanut;

[RegisterComponent]
public sealed partial class PeanutTeleportActionComponent : Component
{
    [DataField]
    public TimeSpan teleportCooldown = TimeSpan.FromSeconds(3f);
}
