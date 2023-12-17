using Content.Server.Chat.Systems;
using Content.Server.Station.Components;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Content.Server.Light.Components;
using Content.Shared.Light.Components;
using Content.Shared.Audio;
using Robust.Server.GameObjects;
using System.Text.RegularExpressions;
using System.Globalization;
using Content.Shared.Coordinates;
using System.Diagnostics;
using Content.Server.Light.EntitySystems;

namespace Content.Server.Time
{
    public sealed partial class DayCycleSystem : EntitySystem
    {
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly PointLightSystem _pointLight = default!;
        private TimeSystem? _timeSystem;
        private PoweredLightSystem? _lightSystem;
        private int _currentHour;
        private double _deltaTime;
        private string? _hexColor;
        private bool _isNight;
        private Dictionary<int, int[]>? _mapColor;
        private static readonly Regex? HexPattern = new Regex(@"^#([A-Fa-f0-9]){6}$");
        private static readonly SoundSpecifier? NightAlert = new SoundPathSpecifier("/Audio/Announcements/nightshift.ogg");
        private static readonly SoundSpecifier? DayAlert = new SoundPathSpecifier("/Audio/Announcements/dayshift.ogg");

        public override void Initialize()
        {
            base.Initialize();
            _timeSystem = _entitySystem.GetEntitySystem<TimeSystem>();
            _lightSystem = _entitySystem.GetEntitySystem<PoweredLightSystem>();
            _currentHour = _timeSystem!.GetStationTime().Hours;
            _hexColor = "#FFFFFF";
            _deltaTime = 0;
            _isNight = false;
            _mapColor = new Dictionary<int, int[]>();
        }
        public override void Update(float frameTime)
        {
            if (_deltaTime >= 1.0)
            {
                _deltaTime = 0;
                _currentHour = _timeSystem!.GetStationTime().Hours;
                foreach (var comp in EntityQuery<DayCycleComponent>())
                {
                    if (comp.isEnabled)
                    {
                        if (EntityManager.TryGetComponent<StationMemberComponent>(comp.Owner, out var station))
                        {
                            if ((_currentHour >= comp.NightStartTime || _currentHour < TimeSpan.FromHours(comp.NightStartTime + comp.NightDuration).Hours) && !_isNight)
                            {
                                if (comp.IsAnnouncementEnabled)
                                {
                                    _chatSystem.DispatchStationAnnouncement(station.Owner,
                                    Loc.GetString("time-night-shift-announcement"),
                                    Loc.GetString("comms-console-announcement-title-centcom"),
                                    true, NightAlert, colorOverride: Color.SkyBlue);
                                }
                                _isNight = true;
                            }
                            else if (_currentHour >= TimeSpan.FromHours(comp.NightStartTime + comp.NightDuration).Hours && _currentHour < comp.NightStartTime && _isNight)
                            {
                                if (comp.IsAnnouncementEnabled)
                                {
                                    _chatSystem.DispatchStationAnnouncement(station.Owner,
                                    Loc.GetString("time-day-shift-announcement"),
                                    Loc.GetString("comms-console-announcement-title-centcom"),
                                    true, DayAlert, colorOverride: Color.OrangeRed);
                                }
                                _isNight = false;
                            }
                            var lightLevel = CalculateLightLevel(comp);
                            var colorLevel = new double[] { 1, 1, 1 };
                            if (comp.IsColorEnabled)
                            {
                                colorLevel = CalculateColorLevel(comp);
                            }
                            if (lightLevel != comp.LightClip || colorLevel[0] != comp.RedClip || colorLevel[1] != comp.BlueClip || colorLevel[2] != comp.GreenClip)
                            {
                                foreach (var light in EntityQuery<PoweredLightComponent>())
                                {
                                    var uid = light.Owner;
                                    var bulbUid = _lightSystem!.GetBulb(uid, light);
                                    if (station.Owner.Equals(uid.ToCoordinates().GetGridUid(_entityManager).GetValueOrDefault()) && bulbUid != null)
                                    {
                                        if (EntityManager.TryGetComponent<LightBulbComponent>(bulbUid.Value, out var bulb) && bulb.State == LightBulbState.Normal)
                                        {
                                            var color = bulb.Color;
                                            if (comp.ColorOverride)
                                            {
                                                if (_hexColor != comp.HexColor)
                                                {
                                                    var match = HexPattern!.Match(comp.HexColor);
                                                    if (match.Success)
                                                    {
                                                        _hexColor = comp.HexColor;
                                                    }
                                                }
                                                color = System.Drawing.Color.FromArgb(int.Parse(_hexColor!.Replace("#", ""), NumberStyles.HexNumber));
                                            }
                                            var red = Math.Min(255, color.RByte * colorLevel[0]);
                                            var green = Math.Min(255, color.GByte * colorLevel[1]);
                                            var blue = Math.Min(255, color.BByte * colorLevel[2]);
                                            if (EntityManager.TryGetComponent(uid, out PointLightComponent? pointLight))
                                            {
                                                _pointLight.SetColor(uid, System.Drawing.Color.FromArgb((int) red, (int) green, (int) blue), pointLight);
                                                _pointLight.SetEnergy(uid, (float) lightLevel, pointLight);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (EntityManager.TryGetComponent<MapLightComponent>(comp.Owner, out var map))
                        {
                            if (!_mapColor!.ContainsKey(map.Owner.Id))
                            {
                                Color color = map.AmbientLightColor;
                                _mapColor.Add(map.Owner.Id, new int[] { color.RByte, color.GByte, color.BByte });
                            }
                            else
                            {
                                var lightLevel = CalculateLightLevel(comp);
                                var colorLevel = CalculateColorLevel(comp);
                                var red = Math.Min(255, _mapColor[map.Owner.Id][0] * lightLevel);
                                var green = Math.Min(255, _mapColor[map.Owner.Id][1] * lightLevel);
                                var blue = Math.Min(255, _mapColor[map.Owner.Id][2] * lightLevel);
                                if (comp.IsColorEnabled)
                                {
                                    red = Math.Min(255, red * colorLevel[0]);
                                    green = Math.Min(255, green * colorLevel[1]);
                                    blue = Math.Min(255, blue * colorLevel[2]);
                                }
                                if (lightLevel != comp.LightClip || colorLevel[0] != comp.RedClip || colorLevel[1] != comp.GreenClip || colorLevel[2] != comp.BlueClip)
                                {
                                    map.AmbientLightColor = System.Drawing.Color.FromArgb((int) red, (int) green, (int) blue);
                                    Dirty(map.Owner, map);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _deltaTime += frameTime;
            }
        }

        public double CalculateLightLevel(DayCycleComponent comp)
        {
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, comp.CycleDuration) * 24;
            var crest = Math.Max(1, comp.PeakLightLevel);
            var shift = Math.Max(0, comp.BaseLightLevel);
            return Math.Min(comp.LightClip, CalculateCurve(time, wave_lenght, crest, shift, 6));
        }

        public double[] CalculateColorLevel(DayCycleComponent comp)
        {
            var wave_lenght = Math.Max(0, comp.CycleDuration) * 24;
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var color_level = new double[3];
            for (var i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0:
                        color_level[i] = Math.Min(comp.RedClip, CalculateCurve(time, wave_lenght, 2, 0.75, 4));
                        break;
                    case 1:
                        color_level[i] = Math.Min(comp.GreenClip, CalculateCurve(time, wave_lenght, 1.85, 0.835, 10));
                        break;
                    case 2:
                        color_level[i] = Math.Min(comp.BlueClip, CalculateCurve(time, wave_lenght / 2, 2.85, 0.65, 2, wave_lenght / 4));
                        break;
                }
            }
            return color_level;
        }

        public static double CalculateCurve(double x, double wave_lenght, double crest, double shift, double exponent, double phase = 0)
        {
            var sen = Math.Pow(Math.Sin((Math.PI * (phase + x)) / wave_lenght), exponent);
            return ((crest - shift) * sen) + shift;
        }
    }
}
