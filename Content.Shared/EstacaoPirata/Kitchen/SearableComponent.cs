using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.EstacaoPirata.Kitchen;

/// <summary>
/// This is used for determining which items are searable,
/// </summary>
[RegisterComponent,NetworkedComponent]
public sealed class SearableComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public SearingState State = SearingState.Raw;

    [DataField("temperatureToSear")]
    [ViewVariables(VVAccess.ReadOnly)]
    public float TemperatureToSear = 335f;

    [DataField("searingAudio")]
    public SoundSpecifier SearingAudio = new SoundPathSpecifier("");

    [DataField("searingCompletedAudio")]
    public SoundSpecifier SearingCompletedAudio = new SoundPathSpecifier("/Audio/EstacaoPirata/Cooking/Griddle/16fthumaf__08_frying-meat.ogg");


}

public enum SearingState
{
    Raw,
    Done,
    Burnt
}
