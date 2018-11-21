using UnityEngine.TestTools.Graphics;

public class LWGraphicsTestSettingsXR : GraphicsTestSettings
{
    public int WaitFrames = 0;

    public LWGraphicsTestSettingsXR()
    {
        ImageComparisonSettings.TargetWidth = 512;
        ImageComparisonSettings.TargetHeight = 512;
        ImageComparisonSettings.AverageCorrectnessThreshold = 0.005f;
        ImageComparisonSettings.PerPixelCorrectnessThreshold = 0.001f;
    }
}
