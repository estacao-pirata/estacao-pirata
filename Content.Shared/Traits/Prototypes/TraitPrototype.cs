using Content.Shared.Customization.Systems;
using Content.Shared.Mood;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization;

namespace Content.Shared.Traits;


/// <summary>
///     Describes a trait.
/// </summary>
[Prototype("trait")]
public sealed partial class TraitPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Which customization tab to place this entry in
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TraitCategoryPrototype> Category = "Uncategorized";

    /// <summary>
    ///     How many points this will give the character
    /// </summary>
    [DataField]
    public int Points = 0;


    [DataField]
    public List<CharacterRequirement> Requirements = new();

    /// <summary>
    ///     The components that get added to the player when they pick this trait.
    /// </summary>
    [DataField]
    public ComponentRegistry? Components { get; private set; } = default!;

    /// <summary>
    ///     The components that will be removed from a player when they pick this trait.
    ///     Primarily used to remove species innate traits.
    /// </summary>
    [DataField]
    public List<string>? ComponentRemovals { get; private set; } = default!;

    /// <summary>
    ///     The list of each Action that this trait adds in the form of ActionId and ActionEntity
    /// </summary>
    [DataField]
    public List<EntProtoId>? Actions { get; private set; } = default!;

    /// <summary>
    ///     The list of all Psionic Powers that this trait adds. If this list is not empty, the trait will also Ensure that a player is Psionic.
    /// </summary>
    [DataField]
    public List<string>? PsionicPowers { get; private set; } = default!;

    /// <summary>
    ///     The list of all Spoken Languages that this trait adds.
    /// </summary>
    [DataField]
    public List<string>? LanguagesSpoken { get; private set; } = default!;

    /// <summary>
    ///     The list of all Understood Languages that this trait adds.
    /// </summary>
    [DataField]
    public List<string>? LanguagesUnderstood { get; private set; } = default!;

    /// <summary>
    ///     The list of all Spoken Languages that this trait removes.
    /// </summary>
    [DataField]
    public List<string>? RemoveLanguagesSpoken { get; private set; } = default!;

    /// <summary>
    ///     The list of all Understood Languages that this trait removes.
    /// </summary>
    [DataField]
    public List<string>? RemoveLanguagesUnderstood { get; private set; } = default!;

    /// <summary>
    ///     The list of all Moodlets that this trait adds.
    /// </summary>
    [DataField]
    public List<ProtoId<MoodEffectPrototype>>? MoodEffects { get; private set; } = default!;

    /// <summary>
    ///     Gear that is given to the player, when they pick this trait.
    /// </summary>
    [DataField]
    public ProtoId<EntityPrototype> TraitGear = default!;
    [DataField(serverOnly: true)]
    public TraitFunction[] Functions { get; private set; } = Array.Empty<TraitFunction>();
}

/// This serves as a hook for trait functions to modify a player character upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class TraitFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
