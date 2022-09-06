using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Speech.Components;

/// <summary>
///     Component required for entities to be able to Fart.
/// </summary>
[RegisterComponent]
public sealed class FartComponent : Component
{
    [DataField("fart")]
    public SoundSpecifier Fart = new SoundCollectionSpecifier("Farts");

    [DataField("audioParams")]
    public AudioParams AudioParams = AudioParams.Default.WithVolume(2f);

    public const float Variation = 0.125f;

    [DataField("actionId", customTypeSerializer: typeof(PrototypeIdSerializer<InstantActionPrototype>))]
    public string ActionId = "Fart";

    [DataField("action")] // must be a data-field to properly save cooldown when saving game state.
    public InstantAction? FartAction = null;
}

public sealed class FartActionEvent : InstantActionEvent { };
