using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Prawn
{
    [RegisterComponent]
    public sealed class PrawnComponent : Component
    {
        /// <summary>
        ///     The action for the Raise Army ability
        /// </summary>
        [DataField("actionRaiseArmy", required: true)]
        public InstantAction ActionRaiseArmy = new();

        /// <summary>
        ///     The amount of thirst one use of Raise Army consumes
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("thirstPerArmyUse", required: true)]
        public float ThirstPerArmyUse = 75f;

        /// <summary>
        ///     The entity prototype of the mob that Raise Army summons
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("armyMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ArmyMobSpawnId = "MobPrawnServant";

        /// <summary>
        ///     The action for the Domain ability
        /// </summary>
        [ViewVariables, DataField("actionDomain", required: true)]
        public InstantAction ActionDomain = new();

        /// <summary>
        ///     The amount of thirst one use of Domain consumes
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("thirstPerDomainUse", required: true)]
        public float ThirstPerDomainUse = 150f;

        /// <summary>
        ///     How many moles of Miasma are released after one us of Domain
        /// </summary>
        [ViewVariables, DataField("molesMiasmaPerDomain")]
        public float MolesMiasmaPerDomain = 100f;
    }
};
