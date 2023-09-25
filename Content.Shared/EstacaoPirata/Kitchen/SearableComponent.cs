using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.EstacaoPirata.Kitchen;

/// <summary>
/// This is used for determining which items are searable,
/// </summary>
[RegisterComponent,NetworkedComponent]
public sealed partial class SearableComponent : Component
{
    [DataField("searingAudio")]
    public SoundSpecifier SearingAudio = new SoundPathSpecifier("");

    [DataField("searingCompletedAudio")]
    public SoundSpecifier SearingCompletedAudio = new SoundPathSpecifier("/Audio/EstacaoPirata/Cooking/Griddle/16fthumaf__08_frying-meat.ogg");


}
// TODO: usar algo assim no futuro
[Serializable, NetSerializable]
public sealed class AboveHotSurface : EntityEventArgs
{
    public EntityUid Searable;
    public EntityUid? HotSurface;
    public bool Entering;

    public AboveHotSurface(EntityUid searable, EntityUid? hotSurface, bool entering)
    {
        Searable = searable;
        HotSurface = hotSurface;
        Entering = entering;
    }

}

public sealed class SearingAnimationMessage : EntityEventArgs
{
    public EntityUid Entity;

    public SearingAnimationMessage(EntityUid entity)
    {
        Entity = entity;
    }
}
