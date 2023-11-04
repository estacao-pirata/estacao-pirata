using Content.Shared.Mind;
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

    // Mind + team
    public readonly Dictionary<EntityUid, int> BloodFamilyTeams = new();

    public Dictionary<IPlayerSession, (EntityUid,MindComponent, int)> BloodFamilyQueue = new();

    [DataField("bloodFamilyPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public string BloodFamilyPrototypeId = "BloodFamily";

    public int TotalBloodFamilyMembers => BloodFamilyMinds.Count; // Alterar isto

    // TODO: colocar os valores certos

    /// <summary>
    /// Max amount members in a family
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxBloodFamily = 3;

    /// <summary>
    /// For every X players, 1 will be a family member
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int PlayersPerFamilyMember = 8;

    /// <summary>
    /// Min number of players who selected Blood Family in character creation. (MUDAR PARA Min amount of members in a family?)
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
    public int MinPlayers = 5;

    /// <summary>
    /// Maximum players to be selected as a blood family member
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxPlayers = 20;

    public enum SelectionState
    {
        WaitingForSpawn = 0,
        ReadyToSelect = 1,
        SelectionMade = 2,
        Error = 3,
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
