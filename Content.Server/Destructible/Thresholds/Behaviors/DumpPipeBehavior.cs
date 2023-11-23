using Content.Server.Atmos.Piping.EntitySystems;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [Serializable]
    [DataDefinition]
    public sealed partial class DumpPipeBehavior : IThresholdBehavior
    {
        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            system.EntityManager.EntitySysManager.GetEntitySystem<AtmosDestroySystem>().PurgeContents(owner);
        }
    }
}
