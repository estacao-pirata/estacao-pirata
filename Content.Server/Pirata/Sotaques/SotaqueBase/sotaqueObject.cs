using System.Collections.Generic;

namespace Pirata.Sotaque.Json;
public sealed class Phrase
{
    public required List<string> og { get; set; }
    public required List<string> mod { get; set; }
}
public sealed class Root
{
    public required List<Word> words { get; set; }
    public required List<Phrase> phrases { get; set; }
    public List<string>? ending { get; set; }
    public List<string>? begin { get; set; }
}
public sealed class Word
{
    public required List<string> og { get; set; }
    public required List<string> mod { get; set; }
}
