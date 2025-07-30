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

        if (parts.Length < 2)
        {
            Console.WriteLine($"Invalid command format: {command}");
            return;
        }

        // Try to parse the main command type
        if (!Enum.TryParse(parts[0], true, out HardwareCommandType commandType)) // `true` for ignoreCase
        {
            Console.WriteLine($"Unknown command type: {parts[0]}");
            return;
        }

        switch (commandType)
        {
            case HardwareCommandType.LED:
                HandleLEDCommand(parts);
                break;
            // Add cases for other HardwareCommandType enums as you implement them
            // case HardwareCommandType.NFC:
            //     HandleNFCCommand(parts);
            //     break;
            default:
                Console.WriteLine($"Command type {commandType} not yet implemented or invalid.");
                break;
        }
    }

    private static void HandleLEDCommand(string[] parts)
    {
        if (parts.Length < 3) // e.g., "LED:ON:1" needs at least 3 parts
        {
            Console.WriteLine($"Invalid LED command format: {string.Join(":", parts)}");
            return;
        }

        if (!Enum.TryParse(parts[1], true, out LED.Enums.LEDAction ledAction)) // `true` for ignoreCase
        {
            Console.WriteLine($"Unknown LED action: {parts[1]}");
            return;
        }

        // Try to parse the channel (most LED commands require a channel)
        // Default to AllChannels if parsing fails or not provided (e.g., for StopAll)
        LED.Enums.LEDChannel channel = LED.Enums.LEDChannel.AllChannels; // Default value
        if (parts.Length > 2 && int.TryParse(parts[2], out int channelInt))
        {
            if (Enum.IsDefined(typeof(LED.Enums.LEDChannel), channelInt))
            {
                channel = (LED.Enums.LEDChannel)channelInt;
            }
            else
            {
                Console.WriteLine($"Invalid LED channel: {channelInt}");
                return;
            }
        }


        switch (ledAction)
        {
            case LED.Enums.LEDAction.On:
                Console.WriteLine($"Turning LED channel {channel} ON");
                _ledController.ApplyPattern((int)channel, new SolidColorPattern(Color.White));
                break;

            case LED.Enums.LEDAction.Off:
                Console.WriteLine($"Turning LED channel {channel} OFF");
                _ledController.StopLoop((int)channel);
                break;

            case LED.Enums.LEDAction.SetColor:
                if (parts.Length == 6 &&
                    byte.TryParse(parts[3], out byte r) && byte.TryParse(parts[4], out byte g) &&
                    byte.TryParse(parts[5], out byte b))
                {
                    Console.WriteLine($"Setting LED channel {channel} to Color({r},{g},{b})");
                    _ledController.ApplyPattern((int)channel, new SolidColorPattern(Color.FromArgb(r, g, b)));
                }
                else
                {
                    Console.WriteLine("Invalid LED:SetColor command format. Expected: LED:SetColor:<channel>:<R>:<G>:<B>");
                }
                break;

            case LED.Enums.LEDAction.ApplyPattern:
                if (parts.Length >= 4) // e.g., "LED:ApplyPattern:1:HEARTBEAT"
                {
                    if (!Enum.TryParse(parts[3], true, out LED.Enums.LEDPatternType patternType))
                    {
                        Console.WriteLine($"Unknown LED pattern type: {parts[3]}");
                        return;
                    }

                    Console.WriteLine($"Applying pattern {patternType} to channel {channel}");

                    switch (patternType)
                    {
                        case LED.Enums.LEDPatternType.Heartbeat:
                            if (parts.Length == 7 &&
                                byte.TryParse(parts[4], out byte hr) && byte.TryParse(parts[5], out byte hg) &&
                                byte.TryParse(parts[6], out byte hb))
                            {
                                _ledController.ApplyPattern((int)channel, new HearthbeatPattern(Color.FromArgb(hr, hg, hb)));
                            }
                            else
                            {
                                Console.WriteLine("Invalid HEARTBEAT pattern format. Expected: LED:ApplyPattern:<channel>:HEARTBEAT:<R>:<G>:<B>");
                            }
                            break;
                        case LED.Enums.LEDPatternType.LoopFade:
                            _ledController.ApplyPattern((int)channel, new LoopFadePattern());
                            break;
                        case LED.Enums.LEDPatternType.Rainbow:
                            _ledController.ApplyPattern((int)channel, new RainbowPattern());
                            break;
                        case LED.Enums.LEDPatternType.RainbowWheel:
                            _ledController.ApplyPattern((int)channel, new RainbowWheelPattern());
                            break;
                        case LED.Enums.LEDPatternType.BigWin:
                            if (parts.Length == 7 &&
                                byte.TryParse(parts[4], out byte br) && byte.TryParse(parts[5], out byte bg) &&
                                byte.TryParse(parts[6], out byte bb))
                            {
                                _ledController.ApplyPattern((int)channel, new BigWinBlinkPattern(Color.FromArgb(br, bg, bb)));
                            }
                            else
                            {
                                Console.WriteLine("Invalid BIG_WIN pattern format. Expected: LED:ApplyPattern:<channel>:BIG_WIN:<R>:<G>:<B>");
                            }
                            break;
                        case LED.Enums.LEDPatternType.Jackpot:
                            // Note: Assuming JackpotPulsePattern in server takes color, not speed directly in constructor currently.
                            // If you add speed, parse parts[7] and pass it.
                            if (parts.Length >= 7 && // Check length for color, possibly +1 for speed
                                byte.TryParse(parts[4], out byte jr) && byte.TryParse(parts[5], out byte jg) &&
                                byte.TryParse(parts[6], out byte jb))
                            {
                                // If you want to use speed:
                                // float speed = 0.05f; // Default
                                // if (parts.Length == 8 && float.TryParse(parts[7], out float parsedSpeed)) {
                                //     speed = parsedSpeed;
                                // }
                                _ledController.ApplyPattern((int)channel, new JackpotPulsePattern(Color.FromArgb(jr, jg, jb))); // Pass speed if constructor updated
                            }
                            else
                            {
                                Console.WriteLine("Invalid JACKPOT pattern format. Expected: LED:ApplyPattern:<channel>:JACKPOT:<R>:<G>:<B>[:<Speed>]");
                            }
                            break;
                        case LED.Enums.LEDPatternType.Chase:
                            // Note: Assuming ChasePattern in server takes color, not speed directly in constructor currently.
                            // If you add speed, parse parts[7] and pass it.
                            if (parts.Length >= 7 && // Check length for color, possibly +1 for speed
                                byte.TryParse(parts[4], out byte chr) && byte.TryParse(parts[5], out byte chg) &&
                                byte.TryParse(parts[6], out byte chb))
                            {
                                // If you want to use speed:
                                // float speed = 0.05f; // Default
                                // if (parts.Length == 8 && float.TryParse(parts[7], out float parsedSpeed)) {
                                //     speed = parsedSpeed;
                                // }
                                _ledController.ApplyPattern((int)channel, new ChasePattern(Color.FromArgb(chr, chg, chb))); // Pass speed if constructor updated
                            }
                            else
                            {
                                Console.WriteLine("Invalid CHASE pattern format. Expected: LED:ApplyPattern:<channel>:CHASE:<R>:<G>:<B>[:<Speed>]");
                            }
                            break;
                        default:
                            Console.WriteLine($"Unknown LED pattern: {patternType}");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid LED:ApplyPattern command format. Expected: LED:ApplyPattern:<channel>:<pattern_name>:[params...]");
                }
                break;

            case LED.Enums.LEDAction.StopChannel:
                Console.WriteLine($"Stopping LED effects on channel {channel}");
                _ledController.StopLoop((int)channel);
                break;

            case LED.Enums.LEDAction.StopAll:
                Console.WriteLine("Stopping all LED effects.");
                _ledController.StopAllLoops();
                break;

            default:
                Console.WriteLine($"Unknown LED action: {ledAction}");
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

    /// <summary>
    /// Defines the general types of hardware commands.
    /// This helps categorize the incoming TCP messages.
    /// </summary>
    public enum HardwareCommandType
    {
        LED,
        NFC,
        BillAcceptor,
        Printer,
        Counter,
        Alarm,
    }