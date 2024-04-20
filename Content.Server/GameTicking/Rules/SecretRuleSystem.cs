using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Presets;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Configuration;
using Robust.Server.Player;

namespace Content.Server.GameTicking.Rules;

public sealed class SecretRuleSystem : GameRuleSystem<SecretRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    protected override void Added(EntityUid uid, SecretRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);
        PickRule(component);
    }

    protected override void Ended(EntityUid uid, SecretRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        foreach (var rule in component.AdditionalGameRules)
        {
            GameTicker.EndGameRule(rule);
        }
    }

    private void PickRule(SecretRuleComponent component)
    {
        // Pirata: Pesadamente modificado pra não sortear modos sem players prontos
        //
        var _playerGameStatuses = _gameTicker._playerGameStatuses;

        // Counts ready players
        var readyPlayers = 0;
        foreach (var session in _playerManager.Sessions)
        {
            if (_playerGameStatuses[session.UserId] == PlayerGameStatus.ReadyToPlay)
                readyPlayers++;
        }

        var presetString = _configurationManager.GetCVar(CCVars.SecretWeightPrototype);
        var presets = _prototypeManager.Index<WeightedRandomPrototype>(presetString);
        var weights = new Dictionary<string, float>(presets.Weights);

        var smallerMinPlayers = int.MaxValue;

        foreach (var preset_ in presets.Weights.Keys)
        {
            var minPlayers = -1;
            if (preset_ == "Traitor")
            {
                minPlayers = _cfg.GetCVar(CCVars.TraitorMinPlayers);
            }
            else if (preset_ == "Zombie")
            {
                minPlayers = _cfg.GetCVar(CCVars.ZombieMinPlayers);
            }
            else
            {
                if (!_prototypeManager.TryIndex<EntityPrototype>(preset_, out var presetProto))
                {
                    continue;
                }
                if (preset_ == "Revolutionary")
                {
                    if (presetProto.TryGetComponent(out RevolutionaryRuleComponent? gameRuleData))
                        minPlayers = gameRuleData.MinPlayers;
                }
                else
                {
                    if (presetProto.TryGetComponent(out GameRuleComponent? gameRuleData))
                        minPlayers = gameRuleData.MinPlayers;
                }
            }

            if (minPlayers != -1 && readyPlayers < minPlayers)
                weights.Remove(preset_);
            if (minPlayers != -1 && minPlayers < smallerMinPlayers){
                smallerMinPlayers = minPlayers;
            }
        }
        string preset;
        if (weights.Count != 0)
        {
            preset = SharedRandomExtensions.Pick(RobustRandom, weights);
        }
        else // If no mode matches, fallback to extended
        {
            preset = "Extended";
            ChatManager.SendAdminAnnouncement(Loc.GetString("preset-not-enough-ready-players",
                ("readyPlayersCount", readyPlayers), ("minimumPlayers", smallerMinPlayers),
                ("presetName", Loc.GetString("secret-title"))));
        }
        Log.Info($"Selected {preset} for secret.");
        _adminLogger.Add(LogType.EventStarted, $"Selected {preset} for secret.");
        _chatManager.SendAdminAnnouncement(Loc.GetString("rule-secret-selected-preset", ("preset", preset)));

        var rules = _prototypeManager.Index<GamePresetPrototype>(preset).Rules;
        foreach (var rule in rules)
        {
            EntityUid ruleEnt;

            // if we're pre-round (i.e. will only be added)
            // then just add rules. if we're added in the middle of the round (or at any other point really)
            // then we want to start them as well
            if (GameTicker.RunLevel <= GameRunLevel.InRound)
                ruleEnt = GameTicker.AddGameRule(rule);
            else
            {
                GameTicker.StartGameRule(rule, out ruleEnt);
            }

            component.AdditionalGameRules.Add(ruleEnt);
        }
    }
}
