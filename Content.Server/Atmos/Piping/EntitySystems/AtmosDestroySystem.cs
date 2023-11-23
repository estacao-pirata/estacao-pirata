using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Piping.EntitySystems
{
    [UsedImplicitly]
    public sealed class AtmosDestroySystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        public void PurgeContents(EntityUid uid){
            if (!EntityManager.TryGetComponent(uid, out NodeContainerComponent? nodes))
                return;
            if (_atmos.GetContainingMixture(uid, true, true) is not {} environment)
                environment = GasMixture.SpaceGas;
            var lost = 0f;
            var timesLost = 0;
            foreach (var node in nodes.Nodes.Values)
            {
                if (node is not PipeNode pipe)
                    continue;
                var difference = pipe.Air.Pressure - environment.Pressure;
                lost += difference * environment.Volume / (environment.Temperature * Atmospherics.R);
                timesLost++;
            }
            var sharedLoss = lost / timesLost;
            var buffer = new GasMixture();
            foreach (var node in nodes.Nodes.Values)
            {
                if (node is not PipeNode pipe)
                    continue;   
                _atmos.Merge(buffer, pipe.Air.Remove(sharedLoss));
            }
            _atmos.Merge(environment, buffer);
        }
    }
}
