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
        AllChannels = 0,
        Channel1 = 1,
        Channel2 = 2,
        Channel3 = 3,
        Channel4 = 4,
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

    // Your existing ProtocolType can remain or be replaced by HardwareCommandType
    public enum ProtocolType
    {
        NFC = 0,
        BILL = 1, // Consider renaming to BillAcceptor for consistency
        PRINTER = 2,
        COUNTER = 3,
        LED = 4,
        ALARM = 5,
    }
}