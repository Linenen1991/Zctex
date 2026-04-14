using System.Drawing;

namespace AsyncElastic;

public sealed class FilterRule
{
    public string ServiceNamePattern { get; set; } = "";
    public string ClassNamePattern { get; set; } = "";
    public string MethodNamePattern { get; set; } = "";
    public string MessagePattern { get; set; } = "";

    public Color MatchColor { get; set; } = Color.LightGoldenrodYellow;
}
