using System.Drawing;
using Quixant.LibRAV;
using qxtraw.Infrastructure.Devices.LED.Presentation;

class Program
{
    private static LEDController _ledController;
    private static IPrinter _printerService;
    private static bool exitRequested = false;


    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting device...");
        PrintWelcome();

        var server = new TcpServer(5000);
        _ledController = new LEDController();
        _printerService = new JCMPrinterImpl();
        var meter = new MetterStepper();
        var deviceManager = new DeviceManager((port) => new JCMDeviceAdapter(port));
        var nfcReader = new NFCReader();
        var doorStatusHandler = new DoorStatusHandler();

        StartDeviceThread(deviceManager);
        StartNfcThread(nfcReader, server);
        StartLedThread();
        StartPrinterThread();
        InitDoorStatusHandler(doorStatusHandler, server);

        var commandHandler = new UnityCommandHandler(_ledController, _printerService);
        server.OnMessageReceived += (_, cmd) => commandHandler.Handle(cmd);

        StartInputThread(deviceManager, _ledController, _printerService, nfcReader, meter);

        _ = server.StartAsync();
        server.OnClientDisconnected += (_, __) => Console.WriteLine("🔄 Unity disconnected. Will wait for reconnection...");
        Console.WriteLine("✅ Server started. Waiting for Unity client...");
    }

    private static void PrintWelcome()
    {
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
    }

    private static void StartDeviceThread(DeviceManager manager)
    {
        var deviceThread = new Thread(() => manager.Initalize());
        deviceThread.Start();
    }

    private static void StartNfcThread(NFCReader reader, TcpServer server)
    {
        var nfcThread = new Thread(() => reader.Init(server));
        nfcThread.Start();
    }

    private static void StartLedThread()
    {
        var ledThread = new Thread(() => _ledController.Init());
        ledThread.Start();
    }

    private static void StartPrinterThread()
    {
        var printerThread = new Thread(() => _printerService.Init());
        printerThread.Start();
    }

    private static void InitDoorStatusHandler(DoorStatusHandler doorHandler, TcpServer server)
    {
        doorHandler.Init();
        doorHandler.OnDoorStatusChangedEvent += async (door, state) =>
        {
            Console.WriteLine($"[DOOR] Door {door} changed to {state}");
            await server.SendMessageAsync($"DOOR:{door}:{state}");
        };
    }

    private static void StartInputThread(DeviceManager manager, LEDController ledController, IPrinter printerService, NFCReader nfcReader, MetterStepper meter)
    {
        var inputThread = new Thread(() => InputLoop(manager, ledController, printerService, nfcReader, meter));
        inputThread.Start();
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
