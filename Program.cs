using System.Drawing;
using Quixant.LibRAV;
using qxtraw.Infrastructure.Devices.LED.Presentation;

class Program
{

    private static LEDController _ledController;
    private static bool exitRequested = false;

    static async Task Main(string[] args)
    {
        var server = new TcpServer(5000);

        Console.WriteLine("Starting device...");
        Console.WriteLine("\nDeviceManager Interactive Console");
        Console.WriteLine("==================================");
        Console.WriteLine("Commands:");
        Console.WriteLine("  [1] Start poll");
        Console.WriteLine("  [2] Stop poll");
        Console.WriteLine("  [3] Return bill");
        Console.WriteLine("  [4] Stack bill extension");
        Console.WriteLine("  [5] Exit");
        Console.WriteLine("  [7] Animate Colors");
        Console.WriteLine("  [8] Dispose Led controller");
        Console.WriteLine("  [9] Print Demo Ticket");

        Console.WriteLine();
        // var deviceManager = new DeviceManager((port) => new MEIDeviceAdapter(port));
        var deviceManager = new DeviceManager((port) => new JCMDeviceAdapter(port));
        var deviceThread = new Thread(() =>
               {
                   deviceManager.Initalize();

               });

        deviceThread.Start();

        var nfcReader = new NFCReader();
        var nfcThread = new Thread(() =>
        {
            nfcReader.Init(server);
        });

        nfcThread.Start();



        server.OnMessageReceived += HandleUnityCommand;


        _ledController = new LEDController();

        var ledThread = new Thread(() =>
        {
            _ledController.Init();
        });

        ledThread.Start();
        IPrinter printerService = new JCMPrinterImpl();

        var printerThread = new Thread(() =>
              {
                  printerService.Init();
                  printerService.PrintDemoTicket();
              });

        printerThread.Start();
        var meter = new MetterStepper();


        var inputThread = new Thread(() => InputLoop(deviceManager, _ledController, printerService, nfcReader, meter));

        inputThread.Start();
        Console.WriteLine("✅ Server started. Waiting for Unity client...");

        _ = server.StartAsync(); // 🔥 Auto-handles reconnect internally

        server.OnClientDisconnected += (_, __) =>
        {
            Console.WriteLine("🔄 Unity disconnected. Will wait for reconnection...");
        };

        Console.WriteLine("🎮 Unity client connected!");

    }

    private static void HandleUnityCommand(object? sender, string command)
    {
        Console.WriteLine($"[COMMAND FROM Client] Processing: {command}");
        string[] parts = command.Split(':');
        string commandType = parts[0].ToUpper();

        switch (commandType)
        {
            case "LED_ON":
                if (parts.Length == 2 && int.TryParse(parts[1], out int onChannel))
                {
                    Console.WriteLine($"Turning LED channel {onChannel} ON");
                    _ledController.ApplyPattern(onChannel, new SolidColorPattern(Color.White)); // Or any default "on" color
                }
                else
                {
                    Console.WriteLine("Invalid LED_ON command format. Expected: LED_ON:<channel>");
                }
                break;

            case "LED_OFF":
                if (parts.Length == 2 && int.TryParse(parts[1], out int offChannel))
                {
                    Console.WriteLine($"Turning LED channel {offChannel} OFF");
                    _ledController.StopLoop(offChannel);

                }
                else
                {
                    Console.WriteLine("Invalid LED_OFF command format. Expected: LED_OFF:<channel>");
                }
                break;

            case "LED_COLOR":
                if (parts.Length == 5 && int.TryParse(parts[1], out int colorChannel) &&
                    byte.TryParse(parts[2], out byte r) && byte.TryParse(parts[3], out byte g) &&
                    byte.TryParse(parts[4], out byte b))
                {
                    Console.WriteLine($"Setting LED channel {colorChannel} to Color({r},{g},{b})");
                    _ledController.ApplyPattern(colorChannel, new SolidColorPattern(Color.FromArgb(r, g, b)));
                }
                else
                {
                    Console.WriteLine("Invalid LED_COLOR command format. Expected: LED_COLOR:<channel>:<R>:<G>:<B>");
                }
                break;

            case "LED_PATTERN":
                if (parts.Length >= 3 && int.TryParse(parts[1], out int patternChannel))
                {
                    string patternName = parts[2].ToUpper();
                    Console.WriteLine($"Applying pattern {patternName} to channel {patternChannel}");

                    switch (patternName)
                    {
                        case "HEARTBEAT":
                            if (parts.Length == 6 &&
                                byte.TryParse(parts[3], out byte hr) && byte.TryParse(parts[4], out byte hg) &&
                                byte.TryParse(parts[5], out byte hb))
                            {
                                _ledController.ApplyPattern(patternChannel, new HearthbeatPattern(Color.FromArgb(hr, hg, hb)));
                            }
                            else
                            {
                                Console.WriteLine("Invalid HEARTBEAT pattern format. Expected: LED_PATTERN:<channel>:HEARTBEAT:<R>:<G>:<B>");
                            }
                            break;
                        case "LOOPFADE":
                            _ledController.ApplyPattern(patternChannel, new LoopFadePattern());
                            break;
                        case "RAINBOW":
                            _ledController.ApplyPattern(patternChannel, new RainbowPattern());
                            break;
                        case "SEGMENTEDCOLORCHASE":
                            _ledController.ApplyPattern(patternChannel, new SegmentedColorChasePattern());
                            break;
                        // Add more cases for other patterns you define
                        default:
                            Console.WriteLine($"Unknown LED pattern: {patternName}");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid LED_PATTERN command format. Expected: LED_PATTERN:<channel>:<pattern_name>:[params...]");
                }
                break;

            case "LED_STOP_CHANNEL":
                if (parts.Length == 2 && int.TryParse(parts[1], out int stopChannel))
                {
                    Console.WriteLine($"Stopping LED effects on channel {stopChannel}");
                    _ledController.StopLoop(stopChannel);
                }
                else
                {
                    Console.WriteLine("Invalid LED_STOP_CHANNEL command format. Expected: LED_STOP_CHANNEL:<channel>");
                }
                break;

            case "LED_STOP_ALL":
                Console.WriteLine("Stopping all LED effects.");
                _ledController.StopAllLoops();
                break;

        }
    }

    private static void InputLoop(DeviceManager manager, LEDController ledController, IPrinter printerService, NFCReader nFCReader, MetterStepper metterStepper)
    {
        while (!exitRequested)
        {
            Console.Write("Main InputLoop() Enter command number: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            switch (input)
            {
                case "1":
                    manager.StartPolling();
                    break;

                case "2":
                    manager.StopPolling();
                    break;

                case "3":
                    manager.ReturnBill();
                    break;

                case "4":
                    manager.StackBill();
                    break;
                case "5":
                    Console.WriteLine("Exiting...");
                    exitRequested = true;
                    manager.StopPolling();
                    nFCReader.StopPolling();
                    return;
                case "7":
                    ledController.ApplyPatterns();
                    break;
                case "8":
                    ledController.DisposeController();
                    break;
                case "9":
                    printerService.PrintDemoTicket();
                    break;
                case "10":
                    metterStepper.TickMeter(0);
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
    }
}
