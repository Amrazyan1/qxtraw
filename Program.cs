using Quixant.Core;

Console.WriteLine("Quixant.LibQxt example - gpio_dout");

Console.Write("Initializing core...");
CoreManager core = CoreManager.GetDefault();
Console.WriteLine("done\n");

bool quit = false;

Console.WriteLine("Available commands: set, clear, blink, quit");
ManualPatternStepper manualPatternStepper = new ManualPatternStepper(core.GPIO, 0, 4, 0xCUL);


while (!quit)
{
    Console.Write("> ");
    string? inputLine = Console.ReadLine();
    if (inputLine == null)
        continue;
    List<string> input = new List<string>(inputLine.Split(' '));

    manualPatternStepper.Step();
    continue;
    if (input.Count == 0)
        continue;

    switch (input[0].ToLower())
    {
        case "quit":
            quit = true;
            break;
        case "set":
        case "clear":
            {
                bool isSet = input[0].ToLower() == "set";

                if (input.Count < 2)
                {
                    Console.WriteLine($"Usage: {input[0].ToLower()} <pin>[,<pin>...]");
                    break;
                }

                string cleanPinText = string.Empty;

                foreach (string pin in input.Skip(1))
                {
                    cleanPinText += pin.Trim();
                }

                List<int> pins = [];

                try
                {
                    pins = cleanPinText.Split(',').Select(x => int.Parse(x)).ToList();
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid pin number");
                    break;
                }


                if (pins.Count == 0)
                {
                    Console.WriteLine("Invalid pin number");
                    break;
                }

                if (pins.Count == 1)
                {
                    Console.Write($"{(isSet ? "SetOutputPin" : "ClearOutputPin")}({pins[0]})...");
                    if (isSet)
                        core.GPIO.SetOutputPin(pins[0]);
                    else
                        core.GPIO.ClearOutputPin(pins[0]);
                    Console.WriteLine("done");
                }
                else
                {
                    ulong mask = 0;
                    ulong value = isSet ? 0xFFFFFFFFFFFFFFFFUL : 0x0UL;

                    foreach (int pin in pins)
                    {
                        mask |= (0x1UL << pin);
                    }

                    Console.Write($"DigitalOutput(0x{mask:X16}, 0x{value:X16})...");
                    core.GPIO.DigitalOutput(value, mask);
                    Console.WriteLine("done");
                }
            }
            break;
        case "blink":
            {
                if (input.Count != 2)
                {
                    Console.WriteLine("Usage: blink <pin>");
                    break;
                }

                int pin = -1;

                try
                {
                    pin = int.Parse(input[1]);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid pin number");
                    break;
                }

                if (pin < 0)
                {
                    Console.WriteLine("Invalid pin number");
                    break;
                }

                Console.Write($"PlayPattern({pin}, 4, 0x000000000000000C)...");
                core.GPIO.PlayPattern(pin, 4, 0xCUL);
                Console.Write("done\nPress ENTER to stop blinking...");
                Console.ReadLine();
                Console.Write($"StopPattern({pin})...");
                core.GPIO.StopPattern(pin);
                Console.WriteLine("done");
            }
            break;
        default:
            Console.WriteLine($"Unknown command: {input[0].ToLower()}");
            break;

    }
}


return 0;