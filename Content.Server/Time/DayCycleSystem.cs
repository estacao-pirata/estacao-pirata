using Content.Server.Chat.Systems;
using Content.Server.Station.Components;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Content.Server.Light.Components;
using Content.Shared.Light.Components;
using Robust.Server.GameObjects;
using System.Text.RegularExpressions;
using System.Globalization;
using Content.Shared.Coordinates;
using Content.Server.Light.EntitySystems;
using Content.Server.Configurable;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using FastAccessors;

namespace Content.Server.Time
{
    public sealed partial class DayCycleSystem : EntitySystem
    {
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IConfigurationManager _configuration = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly PointLightSystem _pointLight = default!;
        private TimeSystem? _timeSystem;
        private PoweredLightSystem? _lightSystem;
        private int _currentHour;
        private double _deltaTime;
        private string? _hexColor;
        private bool _isNight;
        private Dictionary<int, Color>? _mapColor;
        private static readonly Regex? HexPattern = new Regex(@"^#([A-Fa-f0-9]){6}$");
        private static readonly SoundSpecifier? NightAlert = new SoundPathSpecifier("/Audio/Announcements/nightshift.ogg");
        private static readonly SoundSpecifier? DayAlert = new SoundPathSpecifier("/Audio/Announcements/dayshift.ogg");

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<StationMemberComponent, MapInitEvent>(OnMapInit);

            _timeSystem = _entitySystem.GetEntitySystem<TimeSystem>();
            _lightSystem = _entitySystem.GetEntitySystem<PoweredLightSystem>();
            _currentHour = _timeSystem!.GetStationTime().Hours;
            _hexColor = "#FFFFFF";
            _deltaTime = 0;
            _isNight = false;
            _mapColor = new Dictionary<int, Color>();
        }
        private void OnMapInit(EntityUid uid, StationMemberComponent station, MapInitEvent args)
        {
            AddComp<DayCycleComponent>(uid);
            _entityManager.GetComponent<DayCycleComponent>(uid).IsEnabled = _configuration.GetCVar(CCVars.CycleEnabled);
            _entityManager.GetComponent<DayCycleComponent>(uid).IsColorShiftEnabled = _configuration.GetCVar(CCVars.ColorEnabled);
            _entityManager.GetComponent<DayCycleComponent>(uid).IsAnnouncementEnabled = _configuration.GetCVar(CCVars.AnnouncementEnabled);
        }
        public override void Update(float frameTime)
        {
            if (_deltaTime >= 1.0)
            {
                _deltaTime = 0;
                _currentHour = _timeSystem!.GetStationTime().Hours;
                foreach (var comp in EntityQuery<DayCycleComponent>())
                {
                    if (comp.IsEnabled)
                    {
                        if (EntityManager.TryGetComponent<BecomesStationComponent>(comp.Owner, out var station))
                        {
                            if ((_currentHour >= comp.NightShiftStart || _currentHour < TimeSpan.FromHours(comp.NightShiftStart + comp.NightShiftDuration).Hours) && !_isNight)
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
                            else if (_currentHour >= TimeSpan.FromHours(comp.NightShiftStart + comp.NightShiftDuration).Hours && _currentHour < comp.NightShiftStart && _isNight)
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
                            foreach (var light in EntityQuery<PoweredLightComponent>())
                            {
                                var uid = light.Owner;
                                var bulbUid = _lightSystem!.GetBulb(uid, light);
                                if (station.Owner.Equals(uid.ToCoordinates().GetGridUid(_entityManager).GetValueOrDefault()) && bulbUid != null)
                                {
                                    if (EntityManager.TryGetComponent<LightBulbComponent>(bulbUid.Value, out var bulb) && bulb.State == LightBulbState.Normal)
                                    {
                                        var color = bulb.Color;
                                        if (comp.IsOverrideEnabled)
                                        {
                                            if (_hexColor != comp.OverrideColor)
                                            {
                                                var match = HexPattern!.Match(comp.OverrideColor);
                                                if (match.Success)
                                                {
                                                    _hexColor = comp.OverrideColor;
                                                }
                                            }
                                            color = System.Drawing.Color.FromArgb(int.Parse(_hexColor!.Replace("#", ""), NumberStyles.HexNumber));
                                        }
                                        if (EntityManager.TryGetComponent(uid, out PointLightComponent? pointLight))
                                        {
                                            _pointLight.SetColor(uid, GetCycleColor(comp, color), pointLight);
                                            _pointLight.SetEnergy(uid, (float) CalculateLightLevel(comp), pointLight);
                                        }
                                    }
                                }
                            }
                        }
                        if (EntityManager.TryGetComponent<MapLightComponent>(comp.Owner, out var map))
                        {
                            if (!_mapColor!.ContainsKey(map.Owner.Id))
                            {
                                _mapColor.Add(map.Owner.Id, map.AmbientLightColor);
                            }
                            else
                            {
                                var lightLevel = CalculateLightLevel(comp);
                                var color = GetCycleColor(comp, _mapColor[map.Owner.Id]);
                                var red = (int) Math.Min(255, color.RByte * lightLevel);
                                var green = (int) Math.Min(255, color.GByte * lightLevel);
                                var blue = (int) Math.Min(255, color.BByte * lightLevel);
                                map.AmbientLightColor = System.Drawing.Color.FromArgb(red, green, blue);
                                Dirty(map.Owner, map);
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

        public Color GetCycleColor(DayCycleComponent comp, Color color)
        {
            if (comp.IsEnabled && comp.IsColorShiftEnabled)
            {
                var colorLevel = CalculateColorLevel(comp);
                var red = Math.Min(255, color.RByte * colorLevel[0]);
                var green = Math.Min(255, color.GByte * colorLevel[1]);
                var blue = Math.Min(255, color.BByte * colorLevel[2]);
                return System.Drawing.Color.FromArgb((int) red, (int) green, (int) blue);
            }
            else
                return color;

        }

        public Color GetBulbColor(LightBulbComponent bulb)
        {
            if (EntityManager.TryGetComponent<DayCycleComponent>(bulb.Owner.ToCoordinates().GetGridUid(_entityManager), out var comp))
            {
                return GetCycleColor(comp, bulb.Color);
            }
            else
                return bulb.Color;
        }

        public float GetBulbEnergy(LightBulbComponent bulb)
        {
            if (EntityManager.TryGetComponent<DayCycleComponent>(bulb.Owner.ToCoordinates().GetGridUid(_entityManager), out var comp) && comp.IsEnabled)
            {
                return (float) CalculateLightLevel(comp);
            }
            else
                return bulb.LightEnergy;
        }

        public double CalculateLightLevel(DayCycleComponent comp)
        {
            var time = _timeSystem!.GetStationTime().TotalSeconds;
            var wave_lenght = Math.Max(0, comp.CycleDuration) * 24;
            var crest = Math.Max(1, comp.MaxLightLevel);
            var shift = Math.Max(0, comp.MinLightLevel);
            return Math.Min(comp.ClipLight, CalculateCurve(time, wave_lenght, crest, shift, 6));
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
                        color_level[i] = Math.Min(comp.ClipRed, CalculateCurve(time, wave_lenght,
                        Math.Max(0, comp.MaxRedLevel), Math.Max(0, comp.MinRedLevel), 4));
                        break;
                    case 1:
                        color_level[i] = Math.Min(comp.ClipGreen, CalculateCurve(time, wave_lenght,
                        Math.Max(0, comp.MaxGreenLevel), Math.Max(0, comp.MinGreenLevel), 10));
                        break;
                    case 2:
                        color_level[i] = Math.Min(comp.ClipBlue, CalculateCurve(time, wave_lenght / 2,
                        Math.Max(0, comp.MaxBlueLevel), Math.Max(0, comp.MinBlueLevel), 2, wave_lenght / 4));
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
