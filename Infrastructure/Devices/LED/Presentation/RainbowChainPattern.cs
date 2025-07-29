using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

class RainbowChainPattern : ILedPattern
{
    private readonly float _speed;

    public RainbowChainPattern(float speed = 0.02f)
    {
        _speed = speed; // Speed of animation
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        await AnimateBorderRainbow(channel, controller, _speed, cts.Token);
    }

    private async Task AnimateBorderRainbow(int channel, QxLedController controller, float speed, CancellationToken token)
    {
        try
        {
            int count = controller?.GetLedCount(channel) ?? 0;
            if (count <= 0) return;

            float hueStep = 1f / count;  // Spread hue evenly across LEDs
            float shift = 0f;            // Hue offset to create motion

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    // Calculate hue for each LED (rotating)
                    float hue = ((i * hueStep) + shift) % 1f;
                    Color color = HsvToRgb(hue, 1f, 1f);

                    controller?.SetLed(channel, i, color.R, color.G, color.B);
                }

                controller?.MarkDirty(channel);

                // Move the rainbow in one direction
                shift = (shift + speed) % 1f;

                await Task.Delay(TimeSpan.FromSeconds(1f / 30f), token); // ~30 FPS
            }
        }
        catch (OperationCanceledException) { }
    }

    private static Color HsvToRgb(float h, float s, float v)
    {
        int i = (int)(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);

        float r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return Color.FromArgb(
            (int)(r * 255),
            (int)(g * 255),
            (int)(b * 255)
        );
    }
}
