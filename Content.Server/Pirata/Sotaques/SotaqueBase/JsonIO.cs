using System.IO;
using System.Text.Json;

namespace Pirata.Sotaque.Json;
static class jsonIO {
    static public Root readFile(string path) {
        string json = File.ReadAllText(path);

        Root deserialized = JsonSerializer.Deserialize<Root>(json) ?? throw new Exception("Erro: provavelmente algum campo do sotaque do Json está nulo");
        return deserialized;
    }
}
