using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.EstacaoPirata.GameTicking.Rules.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, Access(typeof(BloodFamilyRuleSystem))]
public sealed partial class BloodFamilyRuleComponent : Component
{
    public readonly List<EntityUid> BloodFamilyMinds = new();

    public readonly Dictionary<int, EntityUid> BloodFamilyTeams = new();

    [DataField("bloodFamilyPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public string BloodFamilyPrototypeId = "BloodFamily";

    public int TotalBloodFamilyMembers => BloodFamilyMinds.Count; // Alterar isto

    // TODO: colocar os valores certos

    /// <summary>
    /// Max Blood Family members allowed during selection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxBloodFamily = 3;

    /// <summary>
    /// For every X players, 1 will be a family member
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int PlayersPerFamilyMember = 1;

    /// <summary>
    /// Min number of players who selected Blood Family in character creation.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinBloodFamily = 2;

    /// <summary>
    /// Maximum amount of random objectives a blood family member will have
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxRandomObjectives = 2;

    /// <summary>
    /// Minimum players in game for the game rule to be selected
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinPlayers = 1;

    public enum SelectionState
    {
        WaitingForSpawn = 0,
        ReadyToSelect = 1,
        SelectionMade = 2,
    }

    public SelectionState SelectionStatus = SelectionState.WaitingForSpawn;
    public TimeSpan AnnounceAt = TimeSpan.Zero;
    public Dictionary<IPlayerSession, HumanoidCharacterProfile> StartCandidates = new();

    /// <summary>
    ///     Path to antagonist alert sound.
    /// </summary>
    [DataField("greetSoundNotification")]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Ambience/Antag/traitor_start.ogg");
}
