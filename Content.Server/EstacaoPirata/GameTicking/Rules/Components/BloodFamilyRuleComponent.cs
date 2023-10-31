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

    [DataField("bloodFamilyPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public string BloodFamilyPrototypeId = "BloodFamily";

    public int TotalBloodFamilyMembers => BloodFamilyMinds.Count; // Alterar isto
    //public string[] Codewords = new string[3];

    /// <summary>
    /// Max Blood Family members allowed during selection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxBloodFamily = 3;

    /// <summary>
    /// awawwa
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int PlayersPerFamilyMember = 8;

    /// <summary>
    /// Min Blood Family members needed during selection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinBloodFamily = 2;

    /// <summary>
    ///aa
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxRandomObjectives = 2;

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
