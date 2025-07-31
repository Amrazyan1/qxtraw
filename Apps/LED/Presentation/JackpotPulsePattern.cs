using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace qxtraw.Infrastructure.Devices.LED.Presentation
{
    public class JackpotPulsePattern : ILedPattern
    {
         private readonly Color _color;
    private readonly float _speed;

    public JackpotPulsePattern(Color color, float speed = 0.05f)
    {
        _color = color;
        _speed = speed;
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        int count = controller.GetLedCount(channel);

        while (!cts.Token.IsCancellationRequested)
        {
            for (float t = 0; t < 1 && !cts.Token.IsCancellationRequested; t += _speed)
            {
                byte r = (byte)(_color.R * t);
                byte g = (byte)(_color.G * t);
                byte b = (byte)(_color.B * t);
                controller.SetAllLeds(channel, r, g, b);
                controller.MarkDirty(channel);
                await Task.Delay(20, cts.Token);
            }
            for (float t = 1; t > 0 && !cts.Token.IsCancellationRequested; t -= _speed)
            {
                byte r = (byte)(_color.R * t);
                byte g = (byte)(_color.G * t);
                byte b = (byte)(_color.B * t);
                controller.SetAllLeds(channel, r, g, b);
                controller.MarkDirty(channel);
                await Task.Delay(20, cts.Token);
            }
        }
    }
    }
}