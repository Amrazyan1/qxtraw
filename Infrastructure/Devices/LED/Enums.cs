// File: Service/Enums/HardwareEnums.cs
// You might put this in a shared library or directly in your console app project.

namespace LED.Enums // Replace with your actual service namespace
{
    /// <summary>
    /// Defines the available LED channels on the hardware.
    /// Should match the Unity client's LEDChannel enum.
    /// </summary>
    public enum LEDChannel
    {
        AllChannels = -1,
        Channel0 = 0,
        Channel1 = 1,
        Channel2 = 2,
        Channel3 = 3,
        // Ensure these match Unity
    }

    /// <summary>
    /// Defines the types of LED patterns that can be applied.
    /// Should match the Unity client's LEDPatternType enum.
    /// </summary>
    public enum LEDPatternType
    {
        SolidColor,
        Heartbeat,
        LoopFade,
        Rainbow,
        RainbowWheel,
        BigWin,
        Jackpot,
        Chase,
        // Ensure these match Unity
    }

    /// <summary>
    /// Defines specific LED actions.
    /// </summary>
    public enum LEDAction
    {
        On,
        Off,
        SetColor,
        ApplyPattern,
        StopChannel,
        StopAll,
    }

}