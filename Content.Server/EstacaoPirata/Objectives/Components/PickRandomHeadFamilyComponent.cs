using Content.Server.Objectives.Systems;

namespace Content.Server.EstacaoPirata.Objectives.Components;

/// <summary>
/// This is used for picking a random head exluding members of the blood family to be killed for an objective
/// </summary>
[RegisterComponent, Access(typeof(KillPersonConditionSystem))]
public sealed partial class PickRandomHeadFamilyComponent : Component
{

}
