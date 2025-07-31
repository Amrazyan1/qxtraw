using System;
using System.Drawing;
using qxtraw.Infrastructure.Devices.LED.Presentation;

public static class LEDHandlerUtils
{
    public static void Handle(string[] parts, LEDController controller)
    {
        if (parts.Length < 3)
        {
            Console.WriteLine($"Invalid LED command format: {string.Join(":", parts)}");
            return;
        }

        if (!Enum.TryParse(parts[1], true, out LED.Enums.LEDAction ledAction))
        {
            Console.WriteLine($"Unknown LED action: {parts[1]}");
            return;
        }

        LED.Enums.LEDChannel channel = LED.Enums.LEDChannel.AllChannels;
        if (parts.Length > 2 && int.TryParse(parts[2], out int channelInt) &&
            Enum.IsDefined(typeof(LED.Enums.LEDChannel), channelInt))
        {
            channel = (LED.Enums.LEDChannel)channelInt;
        }

        switch (ledAction)
        {
            case LED.Enums.LEDAction.On:
                controller.ApplyPattern((int)channel, new SolidColorPattern(Color.White));
                break;

            case LED.Enums.LEDAction.Off:
            case LED.Enums.LEDAction.StopChannel:
                controller.StopLoop((int)channel);
                break;

            case LED.Enums.LEDAction.SetColor:
                if (parts.Length == 6 &&
                    byte.TryParse(parts[3], out byte r) &&
                    byte.TryParse(parts[4], out byte g) &&
                    byte.TryParse(parts[5], out byte b))
                {
                    controller.ApplyPattern((int)channel, new SolidColorPattern(Color.FromArgb(r, g, b)));
                }
                else Console.WriteLine("Invalid LED:SetColor format.");
                break;

            case LED.Enums.LEDAction.ApplyPattern:
                HandleApplyPattern(parts, controller, channel);
                break;

            case LED.Enums.LEDAction.StopAll:
                controller.StopAllLoops();
                break;

            default:
                Console.WriteLine($"Unhandled LED action: {ledAction}");
                break;
        }
    }

    private static void HandleApplyPattern(string[] parts, LEDController controller, LED.Enums.LEDChannel channel)
    {
        if (parts.Length < 4 || !Enum.TryParse(parts[3], true, out LED.Enums.LEDPatternType patternType))
        {
            Console.WriteLine("Invalid pattern name or format.");
            return;
        }

        switch (patternType)
        {
            case LED.Enums.LEDPatternType.Heartbeat:
                ApplyRGBPattern(parts, c => new HearthbeatPattern(c));
                break;
            case LED.Enums.LEDPatternType.LoopFade:
                controller.ApplyPattern((int)channel, new LoopFadePattern());
                break;
            case LED.Enums.LEDPatternType.Rainbow:
                controller.ApplyPattern((int)channel, new RainbowPattern());
                break;
            case LED.Enums.LEDPatternType.RainbowWheel:
                controller.ApplyPattern((int)channel, new RainbowWheelPattern());
                break;
            case LED.Enums.LEDPatternType.BigWin:
                ApplyRGBPattern(parts, c => new BigWinBlinkPattern(c));
                break;
            case LED.Enums.LEDPatternType.Jackpot:
                ApplyRGBPattern(parts, c => new JackpotPulsePattern(c));
                break;
            case LED.Enums.LEDPatternType.Chase:
                ApplyRGBPattern(parts, c => new ChasePattern(c));
                break;
            default:
                Console.WriteLine("Unknown LED pattern.");
                break;
        }

        void ApplyRGBPattern(string[] p, Func<Color, ILedPattern> factory)
        {
            if (p.Length >= 7 &&
                byte.TryParse(p[4], out var r) &&
                byte.TryParse(p[5], out var g) &&
                byte.TryParse(p[6], out var b))
            {
                controller.ApplyPattern((int)channel, factory(Color.FromArgb(r, g, b)));
            }
            else Console.WriteLine("Invalid color params for pattern.");
        }
    }
}