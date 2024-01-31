using System.IO;
using System.Text.Json;

namespace Pirata.Sotaque.Json;
static class jsonIO {
    static public Root readFile(string content) {

        Root deserialized = JsonSerializer.Deserialize<Root>(content) ?? throw new Exception("Erro: provavelmente algum campo do sotaque do Json está nulo");
        return deserialized;
    }
}
