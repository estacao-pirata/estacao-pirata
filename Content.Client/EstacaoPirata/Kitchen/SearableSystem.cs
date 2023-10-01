using System.Numerics;
using Content.Shared.EstacaoPirata.Kitchen;
using Content.Shared.EstacaoPirata.Kitchen.Griddle;
using Content.Shared.Popups;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;
using Content.Shared.DrawDepth;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client.EstacaoPirata.Kitchen;

/// <inheritdoc/>
public sealed class SearableSystem : SharedSearableSystem
{

    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly AnimationPlayerSystem _player = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        //base.Initialize();

        SubscribeNetworkEvent<AboveHotSurface>(OnAboveHotSurface); // TODO: pegar evento quando o objeto foi trocado no construction graph
    }

    private void OnAboveHotSurface(AboveHotSurface ev)
    {
        if (Deleted(ev.Searable) || !TryComp<SpriteComponent>(ev.Searable, out var entSprite) || !TryComp<SearableComponent>(ev.Searable, out var searableComponent))
            return;

        if (ev.Entering)
        {
            if (searableComponent.AnimationEntity == null || Deleted(searableComponent.AnimationEntity))
            {
                var transform = Transform(ev.Searable);
                var coordinates = transform.Coordinates;

                // spawnar prototipo da fumaca nas coordenadas do searable? talvez tenha que usar update? n sei

                var animationEnt = Spawn(null, coordinates); // spawna entidade sem prototipo

                searableComponent.AnimationEntity = animationEnt;

                var sprite = AddComp<SpriteComponent>(animationEnt); // adiciona sprite a entidade
                sprite.DrawDepth = (int) DrawDepth.Mobs;
                //_serManager.CopyTo( , ref sprite, notNullableOverride: true); // ??

                if (TryComp<AppearanceComponent>(ev.Searable, out var entAppearance))
                {
                    var appearance = AddComp<AppearanceComponent>(animationEnt);
                    _serManager.CopyTo(entAppearance, ref appearance, notNullableOverride: true);
                }

                var effectLayer = sprite.AddLayer(new SpriteSpecifier.Rsi(new ResPath("EstacaoPirata/Effects/searing_smoke.rsi"), "searing_smoke"));
                sprite.LayerSetOffset(effectLayer, Vector2.Zero + new Vector2(0f, 0.5f));
            }


            _player.Play(searableComponent.AnimationEntity.Value, SearingSmokeAnimation, "searing-smoke-animation");
        }
        else
        {
            if (searableComponent.AnimationEntity != null || !Deleted(searableComponent.AnimationEntity))
            {
                _player.Stop(searableComponent.AnimationEntity.Value, "searing-smoke-animation");
                QueueDel(searableComponent.AnimationEntity.Value);
            }

        }


    }

    private static readonly Animation SearingSmokeAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.8f),
        AnimationTracks =
        {
            new AnimationTrackComponentProperty()
            {
                ComponentType = typeof(SpriteComponent),
                Property = nameof(SpriteComponent.Offset),
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                    new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0), 0.3f),
                    new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0), 0.5f),
                    //
                    // new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                    // new AnimationTrackProperty.KeyFrame(new Vector2(0f, -0.3f), 0.3f),
                    // new AnimationTrackProperty.KeyFrame(new Vector2(0f, 20f), 0.5f),
                    //
                    // new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                    // new AnimationTrackProperty.KeyFrame(new Vector2(0f, -0.3f), 0.3f),
                }
            }
        }
    };
}
