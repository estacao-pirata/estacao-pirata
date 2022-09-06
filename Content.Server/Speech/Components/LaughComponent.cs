using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Speech.Components;

/// <summary>
///     Component required for entities to be able to Laugh.
/// </summary>
[RegisterComponent]
public sealed class LaughComponent : Component
{
    [DataField("maleLaugh")]
    public SoundSpecifier MaleLaugh = new SoundCollectionSpecifier("MaleLaughs");

    [DataField("femaleLaugh")]
    public SoundSpecifier FemaleLaugh = new SoundCollectionSpecifier("FemaleLaughs");

    [DataField("audioParams")]
    public AudioParams AudioParams = AudioParams.Default.WithVolume(2f);

    public const float Variation = 0.125f;

    [DataField("actionId", customTypeSerializer: typeof(PrototypeIdSerializer<InstantActionPrototype>))]
    public string ActionId = "Laugh";

    [DataField("action")] // must be a data-field to properly save cooldown when saving game state.
    public InstantAction? LaughAction = null;
}

public sealed class LaughActionEvent : InstantActionEvent { };
