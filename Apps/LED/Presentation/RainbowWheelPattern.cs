using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading; // Added for CancellationTokenSource
using System.Threading.Tasks;

namespace qxtraw.Infrastructure.Devices.LED.Presentation
{
    public class RainbowWheelPattern : ILedPattern
    {
        public async Task StartAsync(int channel, QxLedController controller, CancellationTokenSource cts)
        {
            int count = controller.GetLedCount(channel);
            float shift = 0;

            while (!cts.Token.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    float hue = ((float)i / count + shift) % 1f;
                    Color c = HsvToRgb(hue, 1, 1);
                    controller.SetLed(channel, i, c.R, c.G, c.B);
                }
                controller.MarkDirty(channel);
                shift += 0.005f; // Smaller shift increment for finer transitions
                await Task.Delay(20, cts.Token); // Shorter delay for higher frame rate
            }
        }

        private Color HsvToRgb(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            v *= 255;
            byte p = (byte)(v * (1 - s));
            byte q = (byte)(v * (1 - f * s));
            byte t = (byte)(v * (1 - (1 - f) * s));
            switch (i % 6)
            {
                case 0: return Color.FromArgb((byte)v, t, p);
                case 1: return Color.FromArgb(q, (byte)v, p);
                case 2: return Color.FromArgb(p, (byte)v, t);
                case 3: return Color.FromArgb(p, q, (byte)v);
                case 4: return Color.FromArgb(t, p, (byte)v);
                default: return Color.FromArgb((byte)v, p, q);
            }
        }
    }


}