using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Ghost.Roles;
using Content.Server.Redial;

namespace Content.Server.Ghost.Roles.UI
{
    public sealed class GhostRolesEui : BaseEui
    {
        private readonly GhostRoleSystem _ghostRoleSystem;

        public GhostRolesEui()
        {
            _ghostRoleSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GhostRoleSystem>();
        }

        public override GhostRolesEuiState GetNewState()
        {
            var enabled = IoCManager.Resolve<RedialManager>().RedialAvailable();
            return new(_ghostRoleSystem.GetGhostRolesInfo(Player), enabled);
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case RequestGhostRoleMessage req:
                    _ghostRoleSystem.Request(Player, req.Identifier);
                    break;
                case FollowGhostRoleMessage req:
                    _ghostRoleSystem.Follow(Player, req.Identifier);
                    break;
                case LeaveGhostRoleRaffleMessage req:
                    _ghostRoleSystem.LeaveRaffle(Player, req.Identifier);
                    break;
            }
        }

        public override void Closed()
        {
            base.Closed();

            _ghostRoleSystem.CloseEui(Player);
        }
    }
}
