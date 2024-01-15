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
        private double _curTime;
        public override void Initialize()
        {
            IoCManager.InjectDependencies(this);
            _curTime = 0;
            _gameTicker = _entitySystem.GetEntitySystem<GameTicker>();
        }
        public double GetRoundDuration()
        {
            return _gameTicker.RoundDuration().TotalSeconds;
        }
        public TimeSpan GetStationTime()
        {
            return TimeSpan.FromSeconds(_curTime);
        }
        private void SetStationTime(double time)
        {
            _curTime = time;
        }
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            SetStationTime(GetRoundDuration() * _cfg.GetCVar(CCVars.TimeScale) + _cfg.GetCVar(CCVars.InitialTime) * _cfg.GetCVar(CCVars.TimeScale));
        }
    }
}
