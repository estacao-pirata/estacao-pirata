using System.Diagnostics;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.EstacaoPirata.Changeling;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Robust.Shared.Prototypes;
using Content.Server.Actions;
using Content.Server.Forensics;
using Content.Shared.DoAfter;
using Content.Server.Mind.Components;
using Content.Shared.Humanoid;
using System.Linq;
using Content.Server.Inventory;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Content.Shared.Store;
using ContainerSystem = Robust.Server.Containers.ContainerSystem;
using TransformSystem = Robust.Server.GameObjects.TransformSystem;
using Content.Server.Mind;

namespace Content.Server.EstacaoPirata.Changeling;
public sealed partial class ChangelingSystem : EntitySystem
{
    //[Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermalImplant = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<ChangelingComponent, InstantActionEvent>(OnActionPerformed); // pra quando usar acao

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAActionEvent>(OnAbsorbAction);

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNADoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<SubdermalImplantComponent, ChangelingShopActionEvent>(OnImplantShop);

        SubscribeLocalEvent<ChangelingComponent, ChangelingArmBladeEvent>(OnArmBlade);

        SubscribeLocalEvent<ChangelingComponent, ChangelingDnaStingEvent>(OnDnaSting);

        SubscribeLocalEvent<SubdermalImplantComponent, ChangelingTransformEvent>(OnTransformImplant);

        SubscribeLocalEvent<StoreComponent, ChangelingSelectTransformEvent>(OnBoughtTransformation);
    }

    // public override void Update(float frameTime)
    // {
    //     base.Update(frameTime);
    //
    //     foreach (var ling in EntityQuery<ChangelingComponent>())
    //     {
    //         if (ling.ChemicalBalance < ling.ChemicalRegenCap)
    //         {
    //             ChangeChemicalAmount(ling.Owner, ling.TransformImplantUid, ling.ChemicalRegenRate, ling, false, true);
    //             //Logger.Info($"Entidade {ling.Owner} esta com {ling.ChemicalBalance} reagentes");
    //         }
    //     }
    // }

    private void OnArmBlade(EntityUid uid, ChangelingComponent component, ChangelingArmBladeEvent args)
    {
        if (!TryComp<HandsComponent>(args.Performer, out var handsComponent))
            return;
        if (handsComponent.ActiveHand == null)
            return;

        var handContainer = handsComponent.ActiveHand.Container;

        if (handContainer == null)
            return;

        // checar se esta ativado
        //component.ArmBladeActivated = !component.ArmBladeActivated;


        // TODO: REFATORAR ISSO TUDO PRA USAR ArmBladeMaxHands, nao ficar spawnando e apagando entidade (usar o pause) e tambem fazer com que não se possa tirar o item da mão que está
        // esse codigo ta muito feio
        // refatorar pra olhar todas as maos pra quando for desativar a lamina
        if (!component.ArmBladeActivated)
        {
            var targetTransformComp = Transform(args.Performer);
            var armbladeEntity = Spawn("TrueArmBlade", targetTransformComp.Coordinates);

            if (handContainer.ContainedEntity != null) // usar foreach pra checar e dropar
            {
                _handsSystem.TryDrop(args.Performer, handsComponent.ActiveHand, targetTransformComp.Coordinates);
            }

            _handsSystem.TryPickup(args.Performer, armbladeEntity);

            component.ArmBladeActivated = true;
        }
        else
        {
            foreach (var item in _hands.EnumerateHeld(uid))
            {

                if (!TryPrototype(item, out var protoInHand))
                    continue;

                var result = protoInHand.ID == "TrueArmBlade";

                if (!result)
                    continue;

                EntityManager.DeleteEntity(item);
                component.ArmBladeActivated = false;
                break;
            }

        }
    }

    private void OnImplantShop(EntityUid uid, SubdermalImplantComponent component, ChangelingShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(args.Performer, uid, store);
    }

    private void OnStartup(EntityUid uid, ChangelingComponent component, ComponentStartup args)
    {
        component.ChemicalBalance = component.StartingChemicals;
        component.PointBalance = component.StartingPoints;
        //update the icon
        //ChangeEssenceAmount(uid, 0, component);

        //AbsorbDNA
        var absorbAction = new EntityTargetAction(_proto.Index<EntityTargetActionPrototype>("AbsorbDNA"))
            {
                CanTargetSelf = false
            };
        _action.AddAction(uid, absorbAction, null);

        // implante da loja
        var coords = Transform(uid).Coordinates;

        var implant = Spawn("ChangelingShopImplant", coords);
        component.StoreImplantUid = implant;
        if (!TryComp<SubdermalImplantComponent>(implant, out var implantComp))
            return;
        _subdermalImplant.ForceImplant(uid, implant, implantComp);
        if (!TryComp<StoreComponent>(implant, out var storeComponent))
            return;
        storeComponent.Categories.Add("ChangelingAbilities");
        storeComponent.CurrencyWhitelist.Add("Points");
        storeComponent.Balance.Add(component.StoreCurrencyName,component.PointBalance);

        // implante do menu de transformacoes
        var transformationsImplant = Spawn("ChangelingTransformationImplant", coords);
        component.TransformImplantUid = transformationsImplant;
        if (!TryComp<SubdermalImplantComponent>(transformationsImplant, out var transImplantComp))
            return;
        _subdermalImplant.ForceImplant(uid, transformationsImplant, transImplantComp);
        if (!TryComp<StoreComponent>(transformationsImplant, out var transformationStoreComponent))
            return;
        transformationStoreComponent.Categories.Add("ChangelingTransformations");
        transformationStoreComponent.CurrencyWhitelist.Add("Chemicals");
        transformationStoreComponent.Balance.Add(component.AbilityCurrencyName,component.ChemicalBalance);


        // TODO: colocar cooldown?
        var dnaStingAction = new EntityTargetAction(_proto.Index<EntityTargetActionPrototype>("ChangelingDnaSting"))
            {
                CanTargetSelf = false
            };
        _action.AddAction(uid, dnaStingAction, null);

        // var transformAction = new InstantAction(_proto.Index<InstantActionPrototype>("ChangelingTransform"));
        // _action.AddAction(uid, transformAction, null);

        TryRegisterHumanoidData(uid, component);

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
        if (args.Handled || args.Cancelled || args.Target == null)
                return;

        TryRegisterHumanoidData(uid, (EntityUid) args.Target,  component);
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
        // if(!HasComp<MindComponent>(target))
        //     return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-dna-failed-nonHumanoid"), user, user);
            return;
        }



        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(user, comp.AbsorbDNADelay, new AbsorbDNADoAfterEvent(), scope, target: target, used: scope)
        {
            BreakOnTargetMove = true,
            BreakOnUserMove = true
        });
    }

    private void OnDnaSting(EntityUid uid, ChangelingComponent component, ChangelingDnaStingEvent args)
    {
        TryRegisterHumanoidData(uid, args.Target, component);
    }

    // register the user on startup
    private void TryRegisterHumanoidData(EntityUid uid, ChangelingComponent comp)
    {
        HumanoidData tempNewHumanoid = new HumanoidData();

        if (!TryComp<MetaDataComponent>(uid, out var targetMeta))
            return;
        if (!TryPrototype(uid, out var prototype, targetMeta))
            return;
        if (!TryComp<DnaComponent>(uid, out var dnaComp))
            return;
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var targetHumAp))
            return;


        tempNewHumanoid.EntityPrototype = prototype;
        tempNewHumanoid.MetaDataComponent = targetMeta;
        tempNewHumanoid.AppearanceComponent = targetHumAp;
        tempNewHumanoid.Dna = dnaComp.DNA;
        tempNewHumanoid.EntityUid = uid;

        //Dirty(user, userMeta); // TENTANDO FAZER FICAR DIRTY

        if (comp.DNAStrandBalance >= comp.DNAStrandCap)
        {
            var lastHumanoidData = comp.StoredHumanoids.Last();
            comp.StoredHumanoids.Remove(lastHumanoidData);
            comp.StoredHumanoids.Add(tempNewHumanoid);
        }
        else
        {
            comp.DNAStrandBalance += 1;
            comp.StoredHumanoids.Add(tempNewHumanoid);
        }
    }

    private void TryRegisterHumanoidData(EntityUid user, EntityUid target, ChangelingComponent comp)
    {
        HumanoidData tempNewHumanoid = new HumanoidData();

        if (!TryComp<MetaDataComponent>(target, out var targetMeta))
        {
            _popup.PopupEntity(Loc.GetString("changeling-dna-failed-impossible"), user, user);
            return;
        }

        if (!TryPrototype(target, out var prototype, targetMeta))
        {
            _popup.PopupEntity(Loc.GetString("changeling-dna-failed-impossible"), user, user);
            return;
        }

        if (!TryComp<DnaComponent>(user, out var dnaComp))
        {
            _popup.PopupEntity(Loc.GetString("changeling-dna-failed-noDna"), user, user);
            return;
        }

        if (!TryComp<HumanoidAppearanceComponent>(target, out var targetHumAp))
        {
            _popup.PopupEntity(Loc.GetString("changeling-dna-failed-nonHumanoid"), user, user);
            return;
        }


        tempNewHumanoid.EntityPrototype = prototype;
        tempNewHumanoid.MetaDataComponent = targetMeta;
        tempNewHumanoid.AppearanceComponent = targetHumAp;
        tempNewHumanoid.Dna = dnaComp.DNA;

        var childUidNullable = SpawnPauseEntity(user, tempNewHumanoid, comp);

        if (childUidNullable == null)
            return;

        var childUid = (EntityUid) childUidNullable;

        tempNewHumanoid.EntityUid = childUid;

        if (comp.DNAStrandBalance >= comp.DNAStrandCap)
        {
            var humanoidDataToRemove = comp.StoredHumanoids.ElementAt(1);
            comp.StoredHumanoids.Remove(humanoidDataToRemove);
            comp.StoredHumanoids.Add(tempNewHumanoid);
        }
        else
        {
            comp.DNAStrandBalance += 1;
            comp.StoredHumanoids.Add(tempNewHumanoid);
        }



        _popup.PopupEntity(Loc.GetString("changeling-dna-obtained", ("target", target)), user, user);
    }

    private void OnTransformImplant(EntityUid uid, SubdermalImplantComponent component, ChangelingTransformEvent args)
    {
        var changelingComponent = EnsureComp<ChangelingComponent>(args.Performer);

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Listings.Clear();

        foreach (var humanoid in changelingComponent.StoredHumanoids)
        {
            if(humanoid.EntityUid == args.Performer)
                continue;

            Logger.Info($"Entity {args.Performer} registered {humanoid.MetaDataComponent?.EntityName}");
            var listingData = new ListingData();
            Debug.Assert(humanoid.MetaDataComponent != null, "humanoid.MetaDataComponent != null");
            listingData.Name = humanoid.MetaDataComponent.EntityName;
            //listingData.Description = humanoid.MetaDataComponent.EntityDescription;
            //TryComp<SpriteComponent>(humanoid.EntityUid, out var spriteComponent);
            //listingData.Icon = spriteComponent.Icon.Default;
            listingData.Categories.Add("ChangelingTransformations");
            var price = FixedPoint2.New(0); // mudar valor pra 5
            listingData.Cost.Add("Chemicals",price);
            object evento = new ChangelingSelectTransformEvent(args.Performer,humanoid.EntityUid); // TODO: fazer isso funcionar
            listingData.ProductEvent = evento;

            store.Listings.Add(listingData);
            //RaiseLocalEvent(args.Performer, evento);

            //store.Listings.Add();
        }
        //store.Listings.Add();

        _store.ToggleUi(args.Performer, uid, store);
    }

    private void OnBoughtTransformation(EntityUid uid, StoreComponent component, ChangelingSelectTransformEvent args)
    {
        var changelingComponent = EnsureComp<ChangelingComponent>(args.Performer);

        var targetHumanoid = changelingComponent.StoredHumanoids.Find(x => x.EntityUid == args.Target);

        RetrievePausedEntity(args.Performer, targetHumanoid, changelingComponent);
    }

    // private void OnTransformImplant(EntityUid uid, ChangelingComponent component, ChangelingTransformEvent args)
    // {
    //
    //     //_euiManager.OpenEui(new AcceptCloningEui(mind, this), client);
    //
    //     var storedHumanoids = component.StoredHumanoids;
    //
    //     if (storedHumanoids.Count < 1)
    //         return;
    //
    //     var firstHumanoid = storedHumanoids.First();
    //     if (firstHumanoid.EntityUid == uid)
    //         firstHumanoid = storedHumanoids.Last();
    //     if (firstHumanoid.EntityUid == uid)
    //         return;
    //
    //     storedHumanoids.Remove(firstHumanoid);
    //     //var targetAppearance = firstHumanoid.AppearanceComponent;
    //
    //
    //     RetrievePausedEntity(uid, firstHumanoid, component);
    // }

    // TODO: passar as actions compradas, quantia de dinheiro, itens na mao como o armblade, passar tambem o implante, sem fazer spawnar outro igual
    private EntityUid? SpawnPauseEntity(EntityUid user, HumanoidData targetHumanoid, ChangelingComponent originalChangelingComponent)
    {
        if(targetHumanoid.EntityPrototype == null ||
           targetHumanoid.AppearanceComponent == null ||
           targetHumanoid.MetaDataComponent == null ||
           targetHumanoid.Dna == null
           )
            return null;

        var targetTransformComp = Transform(user);
        var child = Spawn(targetHumanoid.EntityPrototype.ID, targetTransformComp.Coordinates);

        targetHumanoid.EntityUid = child;

        var transformChild = Transform(child);
        transformChild.LocalRotation = targetTransformComp.LocalRotation;

        var childHumanoidAppearance = EnsureComp<HumanoidAppearanceComponent>(child);
        var childMeta = EnsureComp<MetaDataComponent>(child);
        var childDna = EnsureComp<DnaComponent>(child);

        var targetAppearance = targetHumanoid.AppearanceComponent;

        childHumanoidAppearance.Age = targetAppearance.Age;
        childHumanoidAppearance.BaseLayers = targetAppearance.BaseLayers;
        childHumanoidAppearance.CachedFacialHairColor = targetAppearance.CachedFacialHairColor;
        childHumanoidAppearance.CachedHairColor = targetAppearance.CachedHairColor;
        childHumanoidAppearance.CustomBaseLayers = targetAppearance.CustomBaseLayers;
        childHumanoidAppearance.EyeColor = targetAppearance.EyeColor;
        childHumanoidAppearance.Gender = targetAppearance.Gender;
        childHumanoidAppearance.HiddenLayers = targetAppearance.HiddenLayers;
        //childHumanoidAppearance.Initial = targetAppearance.Initial;
        childHumanoidAppearance.MarkingSet = targetAppearance.MarkingSet;
        childHumanoidAppearance.PermanentlyHidden = targetAppearance.PermanentlyHidden;
        childHumanoidAppearance.Sex = targetAppearance.Sex;
        childHumanoidAppearance.SkinColor = targetAppearance.SkinColor;
        childHumanoidAppearance.Species = targetAppearance.Species;


        childMeta.EntityName = targetHumanoid.MetaDataComponent.EntityName;
        childDna.DNA = targetHumanoid.Dna;

        var changelingComponent = EnsureComp<ChangelingComponent>(child);

        changelingComponent.ArmBladeActivated = originalChangelingComponent.ArmBladeActivated;
        changelingComponent.StoredHumanoids = originalChangelingComponent.StoredHumanoids;
        changelingComponent.DNAStrandBalance = originalChangelingComponent.DNAStrandBalance;
        changelingComponent.ChemicalBalance = originalChangelingComponent.ChemicalBalance;
        changelingComponent.PointBalance = originalChangelingComponent.PointBalance;
        changelingComponent.ArmBladeMaxHands = originalChangelingComponent.ArmBladeMaxHands;

        var userActions = EnsureComp<ActionsComponent>(user);

        var childActions = EnsureComp<ActionsComponent>(child);

        var actionsToPass = new SortedSet<ActionType>();

        bool foundAction = false;

        // passa as acoes de um pra outro
        foreach (var action in userActions.Actions)
        {
            foundAction = false;
            foreach (var childAction in childActions.Actions)
            {
                if (action.DisplayName == childAction.DisplayName)
                {
                    foundAction = true;
                    break;
                }
            }

            if (!foundAction)
                actionsToPass.Add(action);

        }

        foreach (var action in actionsToPass)
        {
            Logger.Info($"Entity {child} recieved action {action.DisplayName}");
            _action.AddAction(child, action, action.Provider);
        }

        SendToPausesMap(child, transformChild);

        return child;

    }

    private void RetrievePausedEntity(EntityUid user, HumanoidData targetHumanoid, ChangelingComponent originalChangelingComponent)
    {
        var childNullable = targetHumanoid.EntityUid;

        // if (childNullable == null)
        //     return;

        var child = (EntityUid) childNullable;

        if (Deleted(child))
            return;

        var childTransform = Transform(child);

        var userTransform = Transform(user);

        _transform.SetParent(child, childTransform, user);
        childTransform.Coordinates = userTransform.Coordinates; // ver esse negocio de obsoleto dps
        childTransform.LocalRotation = userTransform.LocalRotation;

        if (_container.TryGetContainingContainer(user, out var cont))
            cont.Insert(child);

        TransferAllInventory(user,child); //_inventory.TransferEntityInventories(user, child);

        // "transfer" (spawn a new) an unremovable item
        foreach (var item in _hands.EnumerateHeld(user))
        {
            var dropResult = _hands.TryDrop(user, item, checkActionBlocker: false);


            if (!dropResult)
            {

                if(!TryPrototype(item, out var itemPrototype))
                    return;
                var proto = itemPrototype.ID;
                var itemEntity = Spawn(proto, childTransform.Coordinates);
                EntityManager.DeleteEntity(item);
                _handsSystem.TryPickup(child, itemEntity);
            }

            _hands.TryPickupAnyHand(child, item);
        }

        var childChangelingComponent = EnsureComp<ChangelingComponent>(child);

        //changelingComponent = comp;
        childChangelingComponent.ArmBladeActivated = originalChangelingComponent.ArmBladeActivated;
        childChangelingComponent.StoredHumanoids = originalChangelingComponent.StoredHumanoids;
        childChangelingComponent.DNAStrandBalance = originalChangelingComponent.DNAStrandBalance;
        childChangelingComponent.ChemicalBalance = originalChangelingComponent.ChemicalBalance;
        childChangelingComponent.PointBalance = originalChangelingComponent.PointBalance;
        childChangelingComponent.ArmBladeMaxHands = originalChangelingComponent.ArmBladeMaxHands;

        if (!TryComp<StoreComponent>(originalChangelingComponent.StoreImplantUid, out var originalStoreComponent))
            return;

        if (!TryComp<StoreComponent>(childChangelingComponent.StoreImplantUid, out var childStoreComponent))
            return;

        childStoreComponent.Balance = originalStoreComponent.Balance;
        childStoreComponent.Listings = originalStoreComponent.Listings;

        //childStoreComponent.Categories = originalStoreComponent.Categories; // isso aq e interessante passar as categorias de coisa pra vender


        if (TryComp<DamageableComponent>(child, out var damageParent) &&
            _mobThreshold.GetScaledDamage(user, child, out var damage) &&
            damage != null)
        {
            _damageable.SetDamage(child, damageParent, damage);
        }



        if (TryComp<MindContainerComponent>(user, out var mind) && mind.Mind != null)
            _mindSystem.TransferTo(mind.Mind, child);

        _popup.PopupEntity(Loc.GetString("changeling-transformed-successful", ("target", child)), child, child);

        SendToPausesMap(user, userTransform);

        Dirty(child);
    }


    // TODO: pull request to change this on Wizard maybe
    private void TransferAllInventory(EntityUid uid, EntityUid target)
    {
        if (!_inventory.TryGetContainerSlotEnumerator(uid, out var enumerator))
            return;

        Dictionary<string, EntityUid> inventoryEntities = new();
        Dictionary<string, EntityUid> inventoryContainersToDrop = new();
        var slots = _inventory.GetSlots(uid);
        while (enumerator.MoveNext(out var containerSlot))
        {
            //records all the entities stored in each of the target's slots
            foreach (var slot in slots)
            {
                if (_inventory.TryGetSlotContainer(target, slot.Name, out var conslot, out _) &&
                    conslot.ID == containerSlot.ID &&
                    containerSlot.ContainedEntity is { } containedEntity)
                {
                    inventoryEntities.Add(slot.Name, containedEntity);
                }
            }


            inventoryContainersToDrop.Add(containerSlot.ID, uid);
        }

        //drops everything in the target's inventory on the ground
        foreach (var (slot, entity) in inventoryContainersToDrop)
        {
            _inventory.TryUnequip(entity, slot, true, true);
        }

        // This takes the objects we removed and stored earlier
        // and actually equips all of it to the new entity
        foreach (var (slot, item) in inventoryEntities)
        {
            _inventory.TryEquip(target, item, slot , true, true);
        }
    }

    private void SendToPausesMap(EntityUid uid, TransformComponent transform)
    {
        EnsurePausesdMap();

        if (PausedMap == null)
            return;

        _transform.SetParent(uid, transform, PausedMap.Value);
        Logger.Info($"Entidade {uid} spawnada e enviada para o mapa de pause {PausedMap.Value}");
    }

    public bool ChangeChemicalAmount(EntityUid uid, EntityUid? transformationImplant, FixedPoint2 amount, ChangelingComponent? component = null, bool allowDeath = true, bool regenCap = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (transformationImplant == null)
            return false;

        if (!allowDeath && component.ChemicalBalance + amount <= 0)
            return false;

        component.ChemicalBalance += amount;

        if (regenCap)
            FixedPoint2.Min(component.ChemicalBalance, component.ChemicalRegenCap);

        if (TryComp<StoreComponent>(transformationImplant, out var store))
        {
            //store.Balance.Add("Chemicals", component.ChemicalBalance);
            _store.UpdateUserInterface(uid, (EntityUid) transformationImplant, store);
        }


        //_alerts.ShowAlert(uid, AlertType.Essence, (short) Math.Clamp(Math.Round(component.Essence.Float() / 10f), 0, 16));

        // if (component.ChemicalBalance <= 0)
        // {
        //     QueueDel(uid);
        // }
        return true;
    }
}
