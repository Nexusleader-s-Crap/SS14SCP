using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Content.Server._SCP.Scps.oldman.Components;

[RegisterComponent]
public sealed partial class CorrosivePuddleComponent : Component
{
    [DataField]
    public TimeSpan decayTimer = TimeSpan.FromSeconds(5f);

    public TimeSpan decayStart;

    public bool shouldDecay = false;
    public bool isDecaying = false;
}
