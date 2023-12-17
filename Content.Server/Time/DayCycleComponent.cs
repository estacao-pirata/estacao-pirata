
using System.ComponentModel.DataAnnotations;

namespace Content.Server.Time
{
    [RegisterComponent]
    public sealed partial class DayCycleComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField("isEnabled")]
        public bool isEnabled = false;
        [ViewVariables(VVAccess.ReadWrite), DataField("isColorEnabled")]
        public bool IsColorEnabled = false;
        [ViewVariables(VVAccess.ReadWrite), DataField("isAnnouncementEnabled")]
        public bool IsAnnouncementEnabled = false;
        [ViewVariables(VVAccess.ReadWrite), DataField("ColorOverride")]
        public bool ColorOverride = false;
        [ViewVariables(VVAccess.ReadWrite), DataField("Color")]
        public string HexColor = "#FFFFFF";
        [ViewVariables(VVAccess.ReadWrite), DataField("cycleDuration")]
        public double CycleDuration = 3600;
        [ViewVariables(VVAccess.ReadWrite), DataField("nightStartTime")]
        public int NightStartTime = 19;
        [ViewVariables(VVAccess.ReadWrite), DataField("nightDuration")]
        public int NightDuration = 9;
        [ViewVariables(VVAccess.ReadWrite), DataField("peakLightLevel")]
        public double PeakLightLevel = 2;
        [ViewVariables(VVAccess.ReadWrite), DataField("baseLightLevel")]
        public double BaseLightLevel = 0.5;
        [ViewVariables(VVAccess.ReadWrite), DataField("lightClip")]
        public double LightClip = 1.5;
        [ViewVariables(VVAccess.ReadWrite), DataField("redClip")]
        public double RedClip = 1;
        [ViewVariables(VVAccess.ReadWrite), DataField("greenClip")]
        public double GreenClip = 1;
        [ViewVariables(VVAccess.ReadWrite), DataField("blueClip")]
        public double BlueClip = 1.05;
    }
}
