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
         Regex[] ogs = {
            new Regex(@"([aoAO])s($|[bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([aoAO])S($|[bcdfghjklmnpqrtvzxBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([BCDFGHJKLMNPQRSTVXZbcdfghjklmnpqrstvxz])([Ee])s($|[bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([BCDFGHJKLMNPQRSTVXZbcdfghjklmnpqrstvxz])([eE])S($|[bcdfghjklmnpqrtvzxBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([iIuU])s($|[bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([iIuU])S($|[bcdfghjklmnpqrtvzxBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([Ee])s($|[bcdfghjklmnpqrtvxzBCDEFGHJKLMNPQRTVXZ\s\W])"),
            new Regex(@"([Ee])S($|[bcdfghjklmnpqrtvzxBCDEFGHJKLMNPQRTVXZ\s\W])"),
          };
        string[] mods = { "$1ix$2", "$1IX$2", "$1$2ix$3", "$1$2IX$3", "$1x$2", "$1X$2", "ix$2", "IX$2" };
        List<string>moddedToken = new List<string>();
        string[] tokens = message.Split(' ');
        foreach(string token in tokens) {
            bool added = false;
            string currentToken = token;
            for(int i = 0; i < ogs.Length; i++) {
                if(ogs[i].IsMatch(token)) {
                    currentToken = ogs[i].Replace(currentToken, mods[i]);
                    added = true;
                }
            }
            if(!added) {
                moddedToken.Add(token);
            } else {
                moddedToken.Add(currentToken);
            }

                added = false;
        }

        string modded = string.Join(" ", moddedToken);
        return modded;
    }
}
