using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.EstacaoPirata.Kitchen;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedSearableSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly AnimationPlayerSystem _player = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeNetworkEvent<GriddleComponent.BeingGriddledEvent>(OnBeingGriddled);
    }

    private void OnBeingGriddled(GriddleComponent.BeingGriddledEvent ev)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tells clients to play the searing smoke animation.
    /// </summary>
    [Serializable, NetSerializable]
    protected sealed class SearingSmokeAnimationMessage : EntityEventArgs
    {
        public EntityUid Entity;
        public EntityCoordinates Coordinates;
    }
}
