using System;
using qxtraw.Infrastructure.Devices.LED.Presentation;

public class UnityCommandHandler
{
    private readonly LEDController _ledController;
    private readonly IPrinter _printerService;

    public UnityCommandHandler(LEDController ledController, IPrinter printerService)
    {
        _ledController = ledController;
        _printerService = printerService;
    }

    public void Handle(string command)
    {
        Console.WriteLine($"[COMMAND FROM Client] Processing: {command}");

        string[] parts = command.Split(':');
        if (parts.Length < 2)
        {
            Console.WriteLine($"Invalid command format: {command}");
            return;
        }

        if (!Enum.TryParse(parts[0], true, out HardwareCommandType commandType))
        {
            Console.WriteLine($"Unknown command type: {parts[0]}");
            return;
        }

        switch (commandType)
        {
            case HardwareCommandType.LED:
                HandleLEDCommand(parts);
                break;
            case HardwareCommandType.Printer:
                HandlePrinterCommand(parts);
                break;
            default:
                Console.WriteLine($"Command type {commandType} not yet implemented or invalid.");
                break;
        }
    }

    private void HandlePrinterCommand(string[] parts)
    {
        if (parts.Length < 3)
        {
            Console.WriteLine($"Invalid Printer command format: {string.Join(":", parts)}. Expected: Printer:Print:<data>");
            return;
        }

        string printerAction = parts[1];
        if (printerAction.Equals("Print", StringComparison.OrdinalIgnoreCase))
        {
            string dataToPrint = string.Join(":", parts, 2, parts.Length - 2);
            Console.WriteLine($"Received print request for data: {dataToPrint}");
            _printerService.Print(dataToPrint);
        }
        else
        {
            Console.WriteLine($"Unknown Printer action: {printerAction}");
        }
    }

    private void HandleLEDCommand(string[] parts)
    {
        LEDHandlerUtils.Handle(parts, _ledController); // We'll extract this next
    }
}