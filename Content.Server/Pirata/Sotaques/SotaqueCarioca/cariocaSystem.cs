using Robust.Shared.Utility;
using System.Text.RegularExpressions;
using Content.Server.Speech;

namespace Pirata.Sotaque.Carioca;

sealed class SotaqueCarioca : AccentEngine
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SotaqueCariocaComponent, AccentGetEvent>(OnAccent);
    }
    override protected string path { get; set; } = "Resources/Prototypes/EstacaoPirata/Sotaques/Carioca/carioca.json";
    private void OnAccent(EntityUid uid, SotaqueCariocaComponent component, AccentGetEvent args)
    {
        args.Message = Take(args.Message);
    }
    override protected string Parse(string message) {
        //cranking xenophobia to 101% YEEEEAHHHH
        var s = new Regex(@"([aeiouAEIOU])s($|[bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])");
        var S = new Regex(@"([aeiouAEIOU])S($|[bcdfghjklmnpqrtvzxBCDEFGHJKLMNPQRTVXZ\s\W])");
        List<string>moddedToken = new List<string>();
        string[] tokens = message.Split(' ');
        foreach(string token in tokens) {
            if(S.IsMatch(token)) {
                moddedToken.Add(S.Replace(token, "$1X$2"));
                continue;
            }
            if(s.IsMatch(token)) {
                moddedToken.Add(s.Replace(token, "$1x$2"));
                continue;
            }
            moddedToken.Add(token);
        }

        string modded = string.Join(" ", moddedToken);
        return modded;
    }
}
