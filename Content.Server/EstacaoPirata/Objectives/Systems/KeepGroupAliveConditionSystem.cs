using System.Linq;
using Content.Server.EstacaoPirata.GameTicking.Rules;
using Content.Server.EstacaoPirata.GameTicking.Rules.Components;
using Content.Server.EstacaoPirata.Objectives.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.EstacaoPirata.Objectives.Systems;

/// <summary>
/// Handles keep alive condition logic for blood family
/// </summary>
public sealed class KeepGroupAliveConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly GroupTargetObjectiveSystem _groupTarget = default!;
    [Dependency] private readonly BloodFamilyRuleSystem _bloodFamilyRule = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<KeepGroupAliveConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<FamilyAliveComponent, ObjectiveAssignedEvent>(OnAssigned);
    }

    private void OnAssigned(EntityUid uid, FamilyAliveComponent component, ref ObjectiveAssignedEvent args)
    {
        // invalid prototype
        if (!TryComp<GroupTargetObjectiveComponent>(uid, out var comp))
        {
            args.Cancelled = true;
            return;
        }

        var family = Enumerable.ToList<(EntityUid Id, MindComponent Mind)>(_bloodFamilyRule.GetOtherBloodFamilyMindsAliveAndConnected(args.Mind));

        // You are the first/only family member.
        if (family.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        var targets = new List<EntityUid>();
        foreach (var player in family)
        {
            targets.Add(player.Id);
        }

        _groupTarget.SetTargets(uid, targets, comp);
    }

    private void OnGetProgress(EntityUid uid, KeepGroupAliveConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (!_groupTarget.GetTargets(uid, out var targets))
            return;

        args.Progress = GetProgress(targets);
    }

    private float GetProgress(List<EntityUid> targets)
    {
        foreach (var target in targets)
        {
            if (!TryComp<MindComponent>(target, out var mind))
                return 0f;

            if (_mind.IsCharacterDeadIc(mind))
                return 0f;
        }

        return 1f;
    }
}
