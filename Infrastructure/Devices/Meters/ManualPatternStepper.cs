using System;
using Quixant.Core; // Assuming GPIOManager is here

public class ManualPatternStepper
{
    private readonly GPIOManager _gpio;
    private readonly int _pin;
    private readonly ulong _pattern;
    private readonly int _frameCount;
    private int _currentFrame;

    public ManualPatternStepper(GPIOManager gpio, int pin, int frameCount, ulong pattern)
    {
        if (pin < 0 || pin > 63)
            throw new ArgumentOutOfRangeException(nameof(pin));
        if (frameCount < 1 || frameCount > 64)
            throw new ArgumentOutOfRangeException(nameof(frameCount));

        _gpio = gpio;
        _pin = pin;
        _frameCount = frameCount;
        _pattern = pattern;
        _currentFrame = 0;
    }

    /// <summary>
    /// Call this each time you want to step the GPIO by one frame.
    /// </summary>
    public void Step()
    {
        int bitIndex = _currentFrame % _frameCount;
        bool isOn = (_pattern & (1UL << bitIndex)) != 0;

        ulong pinMask = 1UL << _pin;
        ulong value = isOn ? pinMask : 0UL;

        _gpio.DigitalOutput(value, pinMask);
        _currentFrame++;
    }

    public void Reset()
    {
        _currentFrame = 0;
    }

    public int GetCurrentFrame() => _currentFrame % _frameCount;

    public bool GetCurrentState()
    {
        int bitIndex = _currentFrame % _frameCount;
        return (_pattern & (1UL << bitIndex)) != 0;
    }
}