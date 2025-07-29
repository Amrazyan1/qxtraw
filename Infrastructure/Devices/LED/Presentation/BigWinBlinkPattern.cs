using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace qxtraw.Infrastructure.Devices.LED.Presentation
{
   public class BigWinBlinkPattern : ILedPattern
{
    private readonly Color _color;
    private readonly float _speed;

    public BigWinBlinkPattern(Color color, float speed = 0.2f)
    {
        _color = color;
        _speed = speed;
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        while (!cts.Token.IsCancellationRequested)
        {
            controller.SetAllLeds(channel, _color.R, _color.G, _color.B);
            controller.MarkDirty(channel);
            await Task.Delay(TimeSpan.FromSeconds(_speed), cts.Token);

            controller.ClearChannel(channel);
            await Task.Delay(TimeSpan.FromSeconds(_speed), cts.Token);
        }
    }
}
}