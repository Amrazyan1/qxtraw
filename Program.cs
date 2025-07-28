// using Quixant.Core;

// Console.WriteLine("Quixant.LibQxt example - gpio_dout");

// Console.Write("Initializing core...");
// CoreManager core = CoreManager.GetDefault();
// Console.WriteLine("done\n");

// bool quit = false;

// Console.WriteLine("Available commands: set, clear, blink, quit");

// while (!quit)
// {
//     Console.Write("> ");
//     string? inputLine = Console.ReadLine();
//     if (inputLine == null)
//         continue;
//     List<string> input = new List<string>(inputLine.Split(' '));

//     if (input.Count == 0)
//         continue;

//     switch (input[0].ToLower())
//     {
//         case "quit":
//             quit = true;
//             break;
//         case "set":
//         case "clear":
//             {
//                 bool isSet = input[0].ToLower() == "set";

//                 if (input.Count < 2)
//                 {
//                     Console.WriteLine($"Usage: {input[0].ToLower()} <pin>[,<pin>...]");
//                     break;
//                 }

//                 string cleanPinText = string.Empty;

//                 foreach (string pin in input.Skip(1))
//                 {
//                     cleanPinText += pin.Trim();
//                 }

//                 List<int> pins = [];

//                 try
//                 {
//                     pins = cleanPinText.Split(',').Select(x => int.Parse(x)).ToList();
//                 }
//                 catch (Exception)
//                 {
//                     Console.WriteLine("Invalid pin number");
//                     break;
//                 }


//                 if (pins.Count == 0)
//                 {
//                     Console.WriteLine("Invalid pin number");
//                     break;
//                 }

//                 if (pins.Count == 1)
//                 {
//                     Console.Write($"{(isSet ? "SetOutputPin" : "ClearOutputPin")}({pins[0]})...");
//                     if (isSet)
//                         core.GPIO.SetOutputPin(pins[0]);
//                     else
//                         core.GPIO.ClearOutputPin(pins[0]);
//                     Console.WriteLine("done");
//                 }
//                 else
//                 {
//                     ulong mask = 0;
//                     ulong value = isSet ? 0xFFFFFFFFFFFFFFFFUL : 0x0UL;

//                     foreach (int pin in pins)
//                     {
//                         mask |= (0x1UL << pin);
//                     }

//                     Console.Write($"DigitalOutput(0x{mask:X16}, 0x{value:X16})...");
//                     core.GPIO.DigitalOutput(value, mask);
//                     Console.WriteLine("done");
//                 }
//             }
//             break;
//         case "blink":
//             {
//                 if (input.Count != 2)
//                 {
//                     Console.WriteLine("Usage: blink <pin>");
//                     break;
//                 }

//                 int pin = -1;

//                 try
//                 {
//                     pin = int.Parse(input[1]);
//                 }
//                 catch (Exception)
//                 {
//                     Console.WriteLine("Invalid pin number");
//                     break;
//                 }

//                 if (pin < 0)
//                 {
//                     Console.WriteLine("Invalid pin number");
//                     break;
//                 }

//                 Console.Write($"PlayPattern({pin}, 4, 0x000000000000000C)...");
//                 core.GPIO.PlayPattern(pin, 4, 0xCUL);
//                 Console.Write("done\nPress ENTER to stop blinking...");
//                 Console.ReadLine();
//                 Console.Write($"StopPattern({pin})...");
//                 core.GPIO.StopPattern(pin);
//                 Console.WriteLine("done");
//             }
//             break;
//         default:
//             Console.WriteLine($"Unknown command: {input[0].ToLower()}");
//             break;

//     }
// }

// return 0;




//////////
/// 
using Quixant.Core;

Console.WriteLine("Quixant.LibQxt example - gpio_din");

Console.Write("Initializing core...");
//get default core instance
CoreManager core = CoreManager.GetDefault();
bool leave = false;
int fallingEdges = 0, risingEdges = 0, lastFalling = 0, lastRising = 0;
Console.WriteLine("done\n");

Console.WriteLine($"Platform info:     {core.Platform}");
Console.WriteLine($"Driver:            {core.DriverInfo}");
Console.WriteLine($"Library:           {core.NativeLibraryInfo}");
Console.WriteLine($"Device FW:         v{core.FirmwareVersion}");
Console.WriteLine($"Logging Processor: v{core.LoggingProcessor.FirmwareVersion}");
Console.WriteLine($"GPIO:              v{core.GPIO.ModuleVersion}\n");

//set debouncing to 10 for all inputs
core.GPIO.Configure(new(10, 0xFFFFFFFFFFFFFFFFUL));

Console.WriteLine("Press ENTER to start polling digital inputs...");
Console.ReadLine();

ThreadPool.QueueUserWorkItem(_ =>
{
    bool leaveNow = false;
    ulong? oldInputs = null;
    Console.Clear(); //we clear just once to avoid screen flickering

    //install events which will print out GPIO interrupts
    core.GPIO.InputFallingEdgeTriggered += (sender, e) =>
    {
        lock (core)
        {
            fallingEdges++;
            lastFalling = e.Pin;
            Console.Write($"Falling: #{fallingEdges} (last: DIN{lastFalling}) | Rising: #{risingEdges} (last: DIN{lastRising})            \r");
        }
    };
    core.GPIO.InputRisingEdgeTriggered += (sender, e) =>
    {
        lock (core)
        {
            risingEdges++;
            lastRising = e.Pin;
            Console.Write($"Falling: #{fallingEdges} (last: DIN{lastFalling}) | Rising: #{risingEdges} (last: DIN{lastRising})            \r");
        }
    };

    while (!leaveNow)
    {
        lock (core)
        {
            ulong inputs = core.GPIO.DigitalInput();

            if (oldInputs == null || inputs != oldInputs)
            {
                oldInputs = inputs;
                //reporting only 32 lower din
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Monitoring digital inputs\nPress ENTER or ^C to quit...\n\n");
                Console.WriteLine("DIN: 0 |  1 |  2 |  3 |  4 |  5 |  6 |  7 |  8 |  9 | 10 | 11 | 12 | 13 | 14 | 15 ");
                Console.WriteLine("-------+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----");
                Console.WriteLine($"VAL: {(inputs >> 0) & 0x01} |  {(inputs >> 1) & 0x01} |  {(inputs >> 2) & 0x01} |  {(inputs >> 3) & 0x01} |  {(inputs >> 4) & 0x01} |  {(inputs >> 5) & 0x01} |  {(inputs >> 6) & 0x01} |  {(inputs >> 7) & 0x01} |  {(inputs >> 8) & 0x01} |  {(inputs >> 9) & 0x01} |  {(inputs >> 10) & 0x01} |  {(inputs >> 11) & 0x01} |  {(inputs >> 12) & 0x01} |  {(inputs >> 13) & 0x01} |  {(inputs >> 14) & 0x01} |  {(inputs >> 15) & 0x01} ");
                Console.WriteLine("==================================================================================");
                Console.WriteLine("DIN:16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25 | 26 | 27 | 28 | 29 | 30 | 31 ");
                Console.WriteLine("-------+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----");
                Console.WriteLine($"VAL: {(inputs >> 16) & 0x01} |  {(inputs >> 17) & 0x01} |  {(inputs >> 18) & 0x01} |  {(inputs >> 19) & 0x01} |  {(inputs >> 20) & 0x01} |  {(inputs >> 21) & 0x01} |  {(inputs >> 22) & 0x01} |  {(inputs >> 23) & 0x01} |  {(inputs >> 24) & 0x01} |  {(inputs >> 25) & 0x01} |  {(inputs >> 26) & 0x01} |  {(inputs >> 27) & 0x01} |  {(inputs >> 28) & 0x01} |  {(inputs >> 29) & 0x01} |  {(inputs >> 30) & 0x01} |  {(inputs >> 31) & 0x01} ");
                Console.Write($"Falling: #{fallingEdges} (last: DIN{lastFalling}) | Rising: #{risingEdges} (last: DIN{lastRising})            \r");
            }
        }

        Thread.Sleep(200);

        lock (core)
        {
            leaveNow = leave;
        }

    }
});

Console.ReadLine();

lock (core)
{
    leave = true;
}

return 0;