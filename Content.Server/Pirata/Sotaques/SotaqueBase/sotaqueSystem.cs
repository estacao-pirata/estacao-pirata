using System.Linq;
using System.Text.RegularExpressions;
using Pirata.Sotaque.Json;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Pirata.Sotaque;
/// <summary>
/// Classe base para os sotaques do pirata
/// Para criar um novo sotaque, herda dessa classe;
/// Crie um metodo OnAccent, torne-o um evento
/// Caso queira adicionar novos fonemas, modifique o método Parse.
/// </summary>
abstract class AccentEngine : EntitySystem
{
    [Dependency] protected readonly IRobustRandom _random = default!;
    [Dependency] protected readonly IResourceManager _resource = default!;
    virtual protected string path { get; set; } = "";
    virtual protected string file { get; set; } = "";
    virtual protected IEnumerable<ResPath> final_path { get; set; } = default!;
    public string Take(string message)
    {
        //Log.Debug(this.path);
        //Log.Debug(this.file);
        //freakin modify the word
        string[] tokens = message.Split(' ');
        Regex regex = new Regex("\\W+\\Z");
        List<string> moddedTokens = new List<string>();
        Root accent = jsonIO.readFile(this.file);
        foreach (var token in tokens)
        {
            var modifiedToken = "";              //token modificado
            var originalWord = "";               //palavra original
            var punct = "";                             //pontuação
            var punctIndex = 0;
            Match punctMatch = regex.Match(token);  //indice da pontuação?
            if(punctMatch.Success) {
                punctIndex = punctMatch.Index;
                originalWord = token.Remove(punctIndex);
                punct = token.Substring(punctIndex);
            } else {
                originalWord = token;
            }
        //if matching token
        for (int i = 0;  i < accent.words.Count; i++)
        {
            var words = accent.words[i];
            for (int j = 0; j < words.og.Count; j++)
            {
                if (originalWord.ToLower() == words.og[j])
                {
                    modifiedToken = ReplaceWord(originalWord, originalWord.ToLower(), Pick(words.mod));
                    break;
                }
            }
            if(modifiedToken != string.Empty)
            {
                break;
            }
        }
        if(modifiedToken == string.Empty)
            modifiedToken = originalWord;
        // endif
        modifiedToken += punct;
        moddedTokens.Add(modifiedToken);
    }
        string modded = String.Join(" ", moddedTokens);
        for (int i = 0; i < accent.phrases.Count; i++)
        {
            var phrases = accent.phrases[i];
            for (int j = 0; j < phrases.og.Count; j++)
            {
                if (modded.ToLower().Contains(phrases.og[j]))
                {
                    modded = ReplacePhrase(modded, phrases.og[j].ToLower(), Pick(phrases.mod));
                    break;
                }
            }
        }
        int rndNumber = _random.Next(0, 5);
        if (rndNumber == 4)
        {
            if (accent.begin != null)
                modded = Pick(accent.begin) + " " + modded;
        }
        if (rndNumber == 3) {
            if (accent.ending != null)
            {
                var alphanumeric = new Regex(@"\W$");
                if (alphanumeric.IsMatch(modded))
                {
                    modded = modded + " " + Pick(accent.ending);
                } else {
                    modded = modded + ", " + Pick(accent.ending);
                }
            }
        }
        //fckin parse the message for tha goofy accent
        modded = Parse(modded);
        return modded;
    }
    protected string Pick(List<string> tokens)
    {
        int index = _random.Next(tokens.Count);
        return tokens[index];
    }
    abstract protected string Parse(string message);
    string ReplaceWord(string word, string original, string mod)
    {
        string[] token = word.Split(' ');
        string[] modifiedTokens = token;
        List<string> toReplace = new List<string>();
        for (int i = 0; i < token.Length; i++)
        {
            if (token[i].ToLower() == original)
            {
                //if first letter is upper
                if (char.IsUpper(token[i][0]))
                {
                    //if all are upper
                    if (token[i].All(char.IsUpper))
                    {
                        modifiedTokens[i] = mod.ToUpper();
                    } else {
                        modifiedTokens[i] = char.ToUpper(mod[0]) + mod.Substring(1);
                    }
                } else {
                    modifiedTokens[i] = mod;
                }
            } else {
                    modifiedTokens[i] = token[i];
            }
        }
        string message = string.Join(" ", modifiedTokens);
        return message;
    }
    string ReplacePhrase(string phrase, string original, string mod)
    {
        //find de index of the start of the phrase
        //find the length of the phrase
        int index;
        int end = original.Length;
        if (phrase.ToLower().Contains(original))
        {
            index = phrase.ToLower().IndexOf(original);
        } else {
            return phrase;
        }

        string section = phrase.Substring(index, end);
        string moddedSection;
        //if first letter is upper
        if (char.IsUpper(section[0]))
        {
            //if all are upper
            if (section.Replace(" ", "").All(char.IsUpper))
            {
                moddedSection = mod.ToUpper();
            } else {
                moddedSection = char.ToUpper(mod[0]) + mod.Substring(1);
                    }
        } else {
            moddedSection = mod;
        }
        string modded = phrase.Replace(section, moddedSection);
        return modded;
    }
    protected void getPath() {
        var directory = new ResPath(path);
        var final_path = _resource.ContentFindFiles(directory);

        var file = final_path.ElementAt(0);
        this.file = _resource.ContentFileReadText(file).ReadToEnd();
        //Log.Debug(final_path.ElementAt(0).CanonPath);
        //Log.Debug(this.file);
    }
}
