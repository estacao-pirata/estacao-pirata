using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Content.Server.EstacaoPirata.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;

namespace Content.Server.EstacaoPirata.Objectives.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class GroupTargetObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {

        SubscribeLocalEvent<GroupTargetObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAfterAssign(EntityUid uid, GroupTargetObjectiveComponent component, ref ObjectiveAfterAssignEvent args)
    {
        if (!GetTargets(uid, out var targets, component))
            return;

        _metaData.SetEntityName(uid, GetTitle(targets, component.Title), args.Meta);
    }

    public void SetTargets(EntityUid uid, List<EntityUid> targets, GroupTargetObjectiveComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        foreach (var target in targets)
        {
            comp.Targets.Add(target);
        }

    }

    /// <summary>
    /// Gets the targets from the component.
    /// </summary>
    /// <remarks>
    /// If it is null then the prototype is invalid, just return.
    /// </remarks>
    public bool GetTargets(EntityUid uid, [NotNullWhen(true)] out List<EntityUid>? targets, GroupTargetObjectiveComponent? comp = null)
    {
        targets = Resolve(uid, ref comp) ? comp.Targets : null;
        return targets != null;
    }

    private string GetTitle(List<EntityUid> targets, string title)
    {
        var targetsNames = new StringBuilder("");
        int index = 0;
        foreach (var target in targets)
        {
            var targetName = "Unknown";
            if (TryComp<MindComponent>(target, out var mind) && mind.CharacterName != null)
            {
                targetName = mind.CharacterName;
            }

            if(index > 0 && index < targets.Count)
                targetsNames.Append("e ");

            targetsNames.Append(targetName);
            targetsNames.Append(' ');

            index++;
        }
        return Loc.GetString(title, ("targetsNames", targetsNames));
    }
}
