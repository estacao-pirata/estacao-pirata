using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Robust.Client.GameObjects;

namespace Content.Client.EstacaoPirata.Kitchen.Visualizers;

/// <summary>
/// This handles...
/// </summary>
public sealed class SearableVisualizer : VisualizerSystem<SearSmokeVisualsComponent>
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        //SubscribeLocalEvent<GriddleComponent, GriddleComponent.BeingGriddledEvent>(OnStartBeingGriddled);
        SubscribeNetworkEvent<GriddleComponent.BeingGriddledEvent>(OnChangeBeingGriddledState);
        SubscribeNetworkEvent<GriddleComponent.EnterGriddleEvent>(OnEnterGriddle);
        SubscribeNetworkEvent<GriddleComponent.ExitGriddleEvent>(OnExitGriddle);
    }

    private void OnExitGriddle(GriddleComponent.ExitGriddleEvent args)
    {
        Log.Debug("OnExitGriddle CHAMADO");
    }

    private void OnEnterGriddle(GriddleComponent.EnterGriddleEvent args)
    {
        Log.Debug("OnEnterGriddle CHAMADO");
    }

    // Aparentemente isto nao esta sendo chamado?
    private void OnChangeBeingGriddledState(GriddleComponent.BeingGriddledEvent args)
    {
        Log.Debug("OnChangeBeingGriddledState CHAMADO");

        if (args.Occupant == null)
            return;

        TryComp<SpriteComponent>(args.Occupant.Value, out var spriteComponent);

        //spriteComponent.AddLayer();

        if (args.Entering)
        {
            // Mostrar animacao de fumacass
        }
        else
        {
            // Parar de mostrar animacao de fumaca
        }

    }
}
