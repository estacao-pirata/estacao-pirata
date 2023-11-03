using Content.Server.EstacaoPirata.Objectives.Systems;

namespace Content.Server.EstacaoPirata.Objectives.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, Access(typeof(GroupTargetObjectiveSystem))]
public sealed partial class GroupTargetObjectiveComponent : Component
{
    /// <summary>
    /// Locale id for the objective title.
    /// It is passed "targetName" and "job" arguments.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Title = string.Empty;

    /// <summary>
    /// List of targets' uids
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<EntityUid> Targets = new List<EntityUid>();
}
