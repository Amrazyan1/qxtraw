using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace qxtraw.Infrastructure.Devices.LED.Presentation
{
    public class SegmentedColorChasePattern : ILedPattern
    {
         private readonly int _segmentCount;
    private readonly float _speed;

    public SegmentedColorChasePattern(int segmentCount = 6, float speed = 0.1f)
    {
        _segmentCount = segmentCount;
        _speed = speed; // Lower is slower
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        await AnimateColorSegments(channel, controller, _segmentCount, _speed, cts.Token);
    }

    private async Task AnimateColorSegments(int channel, QxLedController controller, int segmentCount, float speed, CancellationToken token)
    {
        try
        {
            int count = controller?.GetLedCount(channel) ?? 0;
            if (count <= 0 || segmentCount <= 0) return;

            int ledsPerSegment = count / segmentCount;
            if (ledsPerSegment <= 0) ledsPerSegment = 1;

            var colors = GetRainbowColors(segmentCount);
            int offset = 0;

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    int segIndex = ((i / ledsPerSegment) + offset) % segmentCount;
                    Color c = colors[segIndex];
                    controller?.SetLed(channel, i, c.R, c.G, c.B);
                }

                controller?.MarkDirty(channel);

                offset = (offset + 1) % segmentCount;

                await Task.Delay(TimeSpan.FromSeconds(speed), token);
            }
        }
        catch (OperationCanceledException) { }
    }

    private List<Color> GetRainbowColors(int count)
    {
        List<Color> colors = new List<Color>();
        for (int i = 0; i < count; i++)
        {
            float hue = i / (float)count;
            colors.Add(HsvToRgb(hue, 1f, 1f));
        }
        return colors;
    }

    public static Color HsvToRgb(float h, float s, float v)
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
}