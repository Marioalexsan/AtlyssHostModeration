using Newtonsoft.Json;

namespace Marioalexsan.HostModeration;

public class OperatorDetails
{
    [JsonProperty(PropertyName = "steamId")]
    public string SteamId { get; set; } = "";

    [JsonProperty(PropertyName = "characterNickname")]
    public string CharacterNickname { get; set; } = "";
}

public class HostModerationData
{
    [JsonProperty(PropertyName = "steamIdOperators")]
    public List<OperatorDetails> SteamIdOperators { get; set; } = [];
}