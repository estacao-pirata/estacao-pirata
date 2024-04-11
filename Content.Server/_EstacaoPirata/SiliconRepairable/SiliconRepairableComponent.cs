using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._EstacaoPirata.SiliconRepairable
{
    [RegisterComponent]
    public sealed partial class SiliconRepairableComponent : Component
    {
        /// <summary>
        ///     All the damage to change information is stored in this <see cref="DamageSpecifier"/>.
        /// </summary>
        /// <remarks>
        ///     If this data-field is specified, it will change damage by this amount instead of setting all damage to 0.
        ///     in order to heal/repair the damage values have to be negative.
        /// </remarks>

        [ViewVariables(VVAccess.ReadWrite)] [DataField("damage")]
        public DamageSpecifier? Damage;

        [ViewVariables(VVAccess.ReadWrite)] [DataField("fuelCost")]
        public int FuelCost = 5;


        //BRUTE, CAUSTIC ( and AIRLOSS, TOXIN, GENETIC, BLEEDING )
        [ViewVariables(VVAccess.ReadWrite)] [DataField("physicalQualityNeeded", customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
        public string PhysicalQualityNeeded = "Welding";


        //BURN
        [ViewVariables(VVAccess.ReadWrite)] [DataField("burnQualityNeeded", customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
        public string BurnQualityNeeded = "CableMending";

        //BLINDNESS
        [ViewVariables(VVAccess.ReadWrite)] [DataField("blindnessQualityNeeded", customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
        public string BlindQualityNeeded = "RepairLens";

        [ViewVariables(VVAccess.ReadWrite)] [DataField("doAfterDelay")]
        public int DoAfterDelay = 1;

        /// <summary>
        /// A multiplier that will be applied to the above if an entity is repairing themselves.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)] [DataField("selfRepairPenalty")]
        public float SelfRepairPenalty = 3f;

        /// <summary>
        /// Whether or not an entity is allowed to repair itself.
        /// </summary>
        [DataField("allowSelfRepair")]
        public bool AllowSelfRepair = true;
    }
}
