using System.Linq;
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
public sealed class KeepFamilyAliveConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<KeepAliveConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<FamilyAliveComponent, ObjectiveAssignedEvent>(OnAssigned);
    }

    private void OnAssigned(EntityUid uid, FamilyAliveComponent component, ref ObjectiveAssignedEvent args)
    {
        var bloodFamilyRuleEntity = EntityQuery<BloodFamilyRuleComponent>().FirstOrDefault();



        throw new NotImplementedException();
    }

    private void OnGetProgress(EntityUid uid, KeepAliveConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        throw new NotImplementedException();
    }
}
