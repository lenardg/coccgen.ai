using System.Diagnostics;
using System.Text.Json.Serialization;

#nullable disable

namespace CoCCGen.Core;

[DebuggerDisplay("{Name} ({DefaultValue})")]
public class COCSkill {

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("specialization")]
    public bool Specialization { get; set; }

    [JsonPropertyName("defaultValue")]
    public int DefaultValue { get; set; }

    [JsonPropertyName("possibleSpecs")]
    public string[] PossibleSpecializations { get; set; }
}
