using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.EstacaoPirata.Changeling;

[Serializable, NetSerializable]
public sealed partial class AbsorbDNADoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class AbsorbDNAActionEvent : EntityTargetActionEvent
{
    //public readonly EntityUid Target;

    // public AbsorbDNAActionEvent(EntityUid target)
    // {
    //     Target = target;
    // }
}

public sealed partial class AbsorbDNADoAfterComplete : EntityEventArgs
{
    public readonly EntityUid Target;

    public AbsorbDNADoAfterComplete(EntityUid target)
    {
        Target = target;
    }
}

public sealed partial class AbsorbDNADoAfterCancelled : EntityEventArgs
{
}

public sealed partial class ChangelingShopActionEvent : InstantActionEvent
{
}

public sealed partial class ChangelingArmBladeEvent : InstantActionEvent
{
}

public sealed partial class ChangelingDnaStingEvent : EntityTargetActionEvent
{
}

public sealed partial class ChangelingFleshmendEvent : InstantActionEvent
{
}

public sealed partial class ChangelingTransformEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ChangelingSelectTransformEvent : EntityEventArgs
{
    public readonly EntityUid Performer;
    public readonly EntityUid Target;
    public ChangelingSelectTransformEvent(EntityUid user, EntityUid target)
    {
        Performer = user;
        Target = target;
    }
}
