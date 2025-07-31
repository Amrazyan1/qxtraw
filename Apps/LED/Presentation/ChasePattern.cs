using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace qxtraw.Infrastructure.Devices.LED.Presentation
{
    public class ChasePattern : ILedPattern
    {
        private readonly Color _color;
    private readonly float _speed;

    public ChasePattern(Color color, float speed = 0.05f)
    {
        _color = color;
        _speed = speed;
    }

    public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
    {
        int count = controller.GetLedCount(channel);
        while (!cts.Token.IsCancellationRequested)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    controller.SetLed(channel, j, (j == i) ? _color.R : (byte)0, (j == i) ? _color.G : (byte)0, (j == i) ? _color.B : (byte)0);
                }
                controller.MarkDirty(channel);
                await Task.Delay((int)(_speed * 1000), cts.Token);
            }
        }
    }
    }
}