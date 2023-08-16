using Robust.Shared.Audio;

namespace Content.Shared.EstacaoPirata.Kitchen.Griddle;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed class GriddleComponent : Component
{
    [DataField("temperatureUpperThreshold")]
    public float TemperatureUpperThreshold = 610.15f;

    [DataField("cookTimeMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float CookTimeMultiplier = 1;

    [DataField("basicFryingAudio")]
    public SoundSpecifier BasicGriddlingAudio = new SoundPathSpecifier("");

    [DataField("fryingCompletedAudio")]
    public SoundSpecifier GriddlingCompletedAudio = new SoundPathSpecifier("/Audio/EstacaoPirata/Cooking/Griddle/16fthumaf__08_frying-meat");

    public sealed class BeingGriddledEvent : HandledEntityEventArgs
    {
        public EntityUid Griddle;
        public EntityUid? Occupant;

        public BeingGriddledEvent(EntityUid griddle, EntityUid? occupant)
        {
            Griddle = griddle;
            Occupant = occupant;
        }
    }
}
