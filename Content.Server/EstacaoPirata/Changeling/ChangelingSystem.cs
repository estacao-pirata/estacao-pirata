using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.EstacaoPirata.Changeling;
using Content.Shared.Interaction;
using Robust.Shared.Player;
using Content.Server.EstacaoPirata.Changeling.Shop;
using Content.Server.Humanoid;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Robust.Shared.Prototypes;
using Content.Server.Actions;
using Content.Server.EstacaoPirata.Changeling.EntitySystems;
using Content.Server.Forensics;
using Content.Shared.DoAfter;
using Content.Server.Mind.Components;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Content.Server.Preferences.Managers;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared.Administration.Logs;
using Content.Server.Speech.Components;
using Content.Server.Traitor;
using Content.Server.UserInterface;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Roles;
using Content.Shared.Store;

namespace Content.Server.EstacaoPirata.Changeling;
public sealed class ChangelingSystem : EntitySystem
{
    [Dependency] private readonly ChangelingShopSystem _changShopSystem = default!;
    [Dependency] private readonly AbsorbSystem _absorbSystem = default!;
    //[Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly EntityManager _entMana = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly SharedImplanterSystem _implanterSystem = default!;
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermalImplant = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<ChangelingComponent, InstantActionEvent>(OnActionPerformed); // pra quando usar acao

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAActionEvent>(OnAbsorbAction);

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<SubdermalImplantComponent, ChangelingShopActionEvent>(OnImplantShop);

        SubscribeLocalEvent<ChangelingComponent, ChangelingArmBladeEvent>(OnArmBlade);

        // Initialize abilities
    }

    private void OnArmBlade(EntityUid uid, ChangelingComponent component, ChangelingArmBladeEvent args)
    {
        // checar se esta comprado?? acho que nao precisa

        // checar se esta ativado
        component.ArmBladeActivated = !component.ArmBladeActivated;

        var targetTransformComp = Transform(args.Performer);
        var armbladeEntity =Spawn("ArmBlade", targetTransformComp.Coordinates);
        _handsSystem.TryPickup(args.Performer, armbladeEntity);
        // if (TryPrototype(uid, out var prototipo, targetMeta))
        // {
        //     var targetTransformComp = Transform(user);
        //     var child = Spawn(prototipo.ID, targetTransformComp.Coordinates);
        //
        // }


    }

    private void OnImplantShop(EntityUid uid, SubdermalImplantComponent component, ChangelingShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(args.Performer, uid, store);
    }

    private void OnStartup(EntityUid uid, ChangelingComponent component, ComponentStartup args)
    {
        //update the icon
        //ChangeEssenceAmount(uid, 0, component);

        //AbsorbDNA
        var absorbaction = new EntityTargetAction(_proto.Index<EntityTargetActionPrototype>("AbsorbDNA"));
        absorbaction.CanTargetSelf = false;
        _action.AddAction(uid, absorbaction, null);

        // implante da loja
        var coords = Transform(uid).Coordinates;

        var implant = Spawn("ChangelingShopImplant", coords);

        if (!TryComp<SubdermalImplantComponent>(implant, out var implantComp))
            return;

        _subdermalImplant.ForceImplant(uid, implant, implantComp);

        if (!TryComp<StoreComponent>(implant, out var storeComponent))
            return;

        storeComponent.Categories.Add("ChangelingAbilities");
        storeComponent.CurrencyWhitelist.Add("Points");

        storeComponent.Balance.Add(component.StoreCurrencyName,component.StartingPoints);

        // Fazer isto em outro lugar

        // if (!TryComp<MindComponent>(uid, out var mindComp))
        //     return;
        // var themind = mindComp.Mind;
        // if (themind is null)
        //     return;
        // var role = new TraitorRole(themind, _proto.Index<AntagPrototype>("Changeling"));
        // themind.AddRole(role);


    }

    private void OnDoAfter(EntityUid uid, ChangelingComponent component, AbsorbDNADoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null)
                return;

        if(!TryComp<HumanoidAppearanceComponent>(args.Target, out var targetHumAp))
            return;

        if(!TryComp<HumanoidAppearanceComponent>(args.User, out var userHumAp))
            return;



        AbsorbDNA(uid, userHumAp, (EntityUid) args.Target, targetHumAp, component);
    }

    // Acho que nao vou usar este
    private void OnActionPerformed(EntityUid uid, ChangelingComponent component, InstantActionEvent args)
    {
    }

    private void OnAbsorbAction(EntityUid uid, ChangelingComponent component, AbsorbDNAActionEvent args)
    {
        StartAbsorbing(uid, args.Performer, args.Target, component);
    }

    private void StartAbsorbing(EntityUid scope, EntityUid user, EntityUid target, ChangelingComponent comp)
    {
        // se nao tiver mente, nao absorver
        if(!HasComp<MindComponent>(target))
            return;

        if(!HasComp<HumanoidAppearanceComponent>(target))
            return;


        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(user, comp.AbsorbDNADelay, new AbsorbDNADoAfterEvent(), scope, target: target, used: scope)
        {
            BreakOnTargetMove = true,
            BreakOnUserMove = true
        });
    }

    private void AbsorbDNA(EntityUid user,HumanoidAppearanceComponent userAppearance, EntityUid target, HumanoidAppearanceComponent targetAppearance, ChangelingComponent comp)
    {
        HumanoidData tempNewHumanoid = new HumanoidData();

        if (!TryComp<MetaDataComponent>(target, out var targetMeta))
            return;
        if (!TryComp<MetaDataComponent>(user, out var userMeta))
            return;
        if (!TryPrototype(target, out var prototype, targetMeta))
            return;
        if (!TryComp<DnaComponent>(user, out var dnaComp))
            return;

        tempNewHumanoid.EntityPrototype = prototype;
        tempNewHumanoid.MetaDataComponent = targetMeta;
        tempNewHumanoid.AppearanceComponent = targetAppearance;
        tempNewHumanoid.Dna = dnaComp.DNA;

        Dirty(user, userMeta); // TENTANDO FAZER FICAR DIRTY

        if (comp.DNAStrandBalance >= comp.DNAStrandCap)
        {
            var tempHumanoidData = comp.storedHumanoids.Last();
            comp.storedHumanoids.Remove(tempHumanoidData);
            comp.storedHumanoids.Add(tempNewHumanoid);
        }
        else
        {
            comp.DNAStrandBalance += 1;
            comp.storedHumanoids.Add(tempNewHumanoid);
        }

        // Coisas para mover para outro metodo
        //userMeta.EntityName = targetMeta.EntityName;
        //_humanoidSystem.CloneAppearance(target, user, targetAppearance, userAppearance);

        // MELHOR: refatorar o MobChangeling, ao inves de fazer um mob especifico para ele, fazer um componente que diz se é ou não apenas
        // isso facilitaria a troca de corpo, pq ai só precisaria criar um novo mob identico ao alvo e passar o componente de changeling pra ele com os valores
        // if (TryPrototype(target, out var prototipo, targetMeta))
        // {
        //     var targetTransformComp = Transform(user);
        //     var child = Spawn(prototipo.ID, targetTransformComp.Coordinates);
        //
        // }
    }

    private void ChangeAppearance(EntityUid user, HumanoidAppearanceComponent userAppearance, EntityUid target, HumanoidAppearanceComponent targetAppearance, ChangelingComponent comp)
    {
        // criar gameobject com os atributos de HumanoidData

        //_humanoidSystem.CloneAppearance(target, user, targetAppearance, userAppearance);
    }
}
