using System;
using Quixant.Core; // Assuming GPIOManager is here

public class MetterStepper
{
    private readonly GPIOManager _gpio;
    const int DELAY_MS = 125;

    public MetterStepper()
    {
        CoreManager core = CoreManager.GetDefault();
        _gpio = core.GPIO;
    }

    public void TickMeter(int pin)
    {
        if (pin < 0 || pin > 63)
            throw new ArgumentOutOfRangeException(nameof(pin));

        ulong mask = 1UL << pin;
        _gpio.DigitalOutput(mask, mask);
        Thread.Sleep(DELAY_MS);
        _gpio.DigitalOutput(0, mask);
        Thread.Sleep(DELAY_MS);
    }

}