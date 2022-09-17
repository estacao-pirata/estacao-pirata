using System.Globalization;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class LobsterAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Dictionary<string, string> DirectReplacements = new()
    {
        { "bom", "pica" },
        { "com raiva", "bolado" },
        { "coisa", "parada" },
        { "legal", "maneiro" },
        { "conversar", "trocar uma ideia"},
        { "presta atenção", "se liga" },
        { "mentira", "caô"},
        { "me ajuda", "da uma moral"},
        { "zuando", "tirando onda" },
        { "sair", "rolé" },
        { "corre", "mete o pé" },
        { "dificil", "sinistro" },
        { "obrigado", "valeu" },
        { "certo", "formô" }
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LobsterAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, LobsterAccentComponent component)
    {
        // Order:
        // Do text manipulations first
        // Then prefix/suffix funnyies

        var msg = message;

        foreach (var (first, replace) in DirectReplacements)
        {
            msg = Regex.Replace(msg, $@"(?<!\w){first}(?!\w)", replace, RegexOptions.IgnoreCase);
        }

        // troca s no meio de frase por x (ixqueiro)
        msg = Regex.Replace(msg, @"(?<=\w)s(?=\w)", "x", RegexOptions.IgnoreCase);

        // Prefix
        if (_random.Prob(0.15f))
        {
            var pick = _random.Next(1, 2);

            // Reverse sanitize capital
            msg = msg[0].ToString().ToLower() + msg.Remove(0, 1);
            msg = Loc.GetString($"accent-lobster-prefix-{pick}") + " " + msg;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        msg = msg[0].ToString().ToUpper() + msg.Remove(0, 1);

        // Suffixes
        if (_random.Prob(0.4f))
        {
            if (component.IsBoss)
            {
                var pick = _random.Next(1, 4);
                msg += Loc.GetString($"accent-lobster-suffix-boss-{pick}");
            }
            else
            {
                var pick = _random.Next(1, 3);
                msg += Loc.GetString($"accent-lobster-suffix-minion-{pick}");
            }
        }

        return msg;
    }

    private void OnAccentGet(EntityUid uid, LobsterAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
