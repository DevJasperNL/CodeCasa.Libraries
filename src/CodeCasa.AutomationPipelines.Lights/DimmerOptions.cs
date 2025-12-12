using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights;

public record DimmerOptions
{
    public int MinBrightness { get; set; } = 2;
    public int BrightnessStep { get; set; } = 51;
    public TimeSpan TimeBetweenSteps { get; set; } = TimeSpan.FromMilliseconds(500);
    public IEnumerable<string>? DimOrderLightEntities { get; set; }

    public void SetLightOrder(IEnumerable<ILight> lightEntities) =>
        DimOrderLightEntities = lightEntities.Select(l => l.Id).ToArray();

    public void SetLightOrder(params ILight[] lightEntities) =>
        DimOrderLightEntities = lightEntities.Select(l => l.Id).ToArray();
}