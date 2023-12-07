using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;


namespace Content.Server.Time
{
    public sealed partial class TimeSystem : EntitySystem
    {
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        private GameTicker _gameTicker = default!;
        private TimeSpan _stationTime;
        private DateTime _baseDateTime;
        private int _initialTime;
        private int _timeAcceleration;

        public override void Initialize()
        {
            IoCManager.InjectDependencies(this);
            _gameTicker = _entitySystem.GetEntitySystem<GameTicker>();
            _stationTime = _gameTicker.RoundDuration();
            _initialTime = _cfg.GetCVar(CCVars.InitialTime);
            _timeAcceleration = _cfg.GetCVar(CCVars.TimeAcceleration);
            _baseDateTime = new DateTime();
        }
        public TimeSpan GetStationSyncTime()
        {
            return _stationTime;
        }
        public TimeSpan GetStationRelativeTime()
        {
            return TimeSpan.FromSeconds(_initialTime + (GetStationSyncTime().TotalSeconds * _timeAcceleration));
        }
        public DateTime GetStationDate()
        {
            return _baseDateTime.Add(GetStationRelativeTime());
        }
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            _stationTime = _gameTicker.RoundDuration();
            _initialTime = _cfg.GetCVar(CCVars.InitialTime);
            _timeAcceleration = _cfg.GetCVar(CCVars.TimeAcceleration);
        }
    }
}