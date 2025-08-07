using System.Diagnostics;
using Quixant.LibRAV;

class JCMDeviceAdapter : IDeviceAdapter
{
    private readonly RAVDevice _device;

    public bool IsOpen => _device.IsOpen;

    public bool _isPolling = true;
    public bool IsPolling { get => _isPolling; set => _isPolling = value; }
    private SerialPortIndex _port;
    public SerialPortIndex port { get => _port; set => _port = value; }

    private int _threadSleepTime = 500;

    public event Action<string> OnStacked;
    public event Action<string> OnReturned;
    public event Action<string> OnEscrowed;
    public event Action<string> OnAccepted;
    public event Action<string> OnIdling;
    public event Action<string> OnJammed;
    public event Action<string> OnCheated;
    public event Action<string> OnRejected;
    public event Action<string> OnStackerFull;
    public event Action<string> OnPaused;
    public event Action<string> OnCalibration;
    public event Action<string> OnPowerUp;
    public event Action<string> OnInvalidCommand;
    public event Action<string> OnFailure;

    public JCMDeviceAdapter(SerialPortIndex port)
    {
        _device = new RAVDevice(port, ProtocolIdentifier.JCM);
        _port = port;
    }

    public void Init()
    {
        enableInterrupts(_device);
        Console.WriteLine("JCMDeviceAdapter Init() starting...");
        Stopwatch sw = Stopwatch.StartNew();
        bool success = false;

        // --- 1. Define all commands ---
        JCMCommand getStatus = new JCMCommand(JCMInstruction.GetStatus, 0, 128);
        JCMCommand reset = new JCMCommand(JCMInstruction.Reset, 0, 0);

        // Configuration Commands
        JCMCommand setEnableDisable = new JCMCommand(JCMInstruction.SetEnableDisable, 2, 0);
        setEnableDisable.InputBuffer[0] = 0x00;
        setEnableDisable.InputBuffer[1] = 0x00;

        JCMCommand setSecurity = new JCMCommand(JCMInstruction.SetSecurity, 2, 0);
        setSecurity.InputBuffer[0] = 0x00;
        setSecurity.InputBuffer[1] = 0x00;

        JCMCommand setOptionalFunction = new JCMCommand(JCMInstruction.SetOptionalFunction, 2, 0);
        setOptionalFunction.InputBuffer[0] = 0x03; // Enable four-way acceptance

        JCMCommand setInhibit = new JCMCommand(JCMInstruction.SetInhibit, 1, 0);
        setInhibit.InputBuffer[0] = 0x00; // Enable ALL banknote denominations

        JCMCommand setBarcodeFunction = new JCMCommand(JCMInstruction.SetBarcodeFunction, 2, 0);
        setBarcodeFunction.InputBuffer[0] = 0x01; // Enable barcode reader
        setBarcodeFunction.InputBuffer[1] = 0x12;

        JCMCommand setBarInhibit = new JCMCommand(JCMInstruction.SetBarInhibit, 1, 0);
        setBarInhibit.InputBuffer[0] = 0xFC;

        try
        {
            // --- 2. Reset the device ---
            Console.WriteLine("JCMDeviceAdapter Init() Sending Reset command...");
            _device.Execute(reset);

            // --- 3. WAIT for the device to finish initializing ---
            Console.Write("JCMDeviceAdapter Init() Waiting for device to become ready");
            int retries = 0;
            do
            {
                Thread.Sleep(200); // Give the device time to process between polls
                _device.Get(getStatus);
                retries++;

                if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Initializing)
                {
                    Console.Write(".");
                }

            } while (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Initializing && retries < 100);

            Console.WriteLine(); // New line after the waiting dots..

            // --- 4. Check if the device is ready or timed out ---
            byte finalStatus = getStatus.OutputBuffer[0];
            if (finalStatus == (byte)JCMStatusResponse.Initializing)
            {
                throw new Exception("Device timed out and is still initializing.");
            }

            Console.WriteLine($"JCMDeviceAdapter Init() Device is ready. Status: 0x{finalStatus:X2}");

            // --- 5. NOW that it's ready, send all configuration commands ---
            Console.WriteLine("JCMDeviceAdapter Init() Sending configuration commands...");
            _device.Set(setEnableDisable);
            _device.Set(setSecurity);
            _device.Set(setOptionalFunction);
            _device.Set(setInhibit);
            _device.Set(setBarcodeFunction);
            _device.Set(setBarInhibit);

            success = true;
        }
        catch (Exception exc)
        {
            Console.WriteLine($"\nJCMDeviceAdapter Init() Test failed: {exc.Message}");
        }
        finally
        {
            sw.Stop();
        }

        if (success)
        {
            Console.WriteLine("\nJCMDeviceAdapter Init() Test succeeded.");
            printTime(sw.ElapsedTicks, 1);
        }
    }


    public void Open()
    {
        if (_device.IsOpen)
        {
            Console.WriteLine($"JCMDeviceAdapter Open() Device on port {port.Name} already open.");
            return;
        }
        Console.WriteLine($"JCMDeviceAdapter Open() Initializing {_device.Protocol} _device on port {port.Name}...");

        try
        {
            _device.Open(port.Name);

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"JCMDeviceAdapter Open() Error initializing _device: {ex.Message}");
        }
    }

    public void Poll()
    {
        const int AcceptTimeout = 50;
        const int EscrowTimeout = 10;
        const int VendValidTimeout = 10;
        Stopwatch sw;
        Console.WriteLine("JCMDeviceAdapter Poll() Please insert bill...");

        var getStatus = new JCMCommand(JCMInstruction.GetStatus, 0, 128);
        var stack1 = new JCMCommand(JCMInstruction.Stack1, 0, 0);
        var ack = new JCMCommand(JCMInstruction.Ack, 0, 0);
        var setInhibit = new JCMCommand(JCMInstruction.SetInhibit, 1, 0);
        setInhibit.InputBuffer[0] = 0xFF; // Enable all channels

        while (IsPolling)
        {
            try
            {
                sw = Stopwatch.StartNew();
                Console.WriteLine("JCMDeviceAdapter Poll() Polling started...");

                if (!WaitForStatus(getStatus, out var status, AcceptTimeout, JCMStatusResponse.Accepting, JCMStatusResponse.Escrow))
                {
                    if (status == JCMStatusResponse.Disabled)
                    {
                        Console.WriteLine("Device disabled. Sending SetInhibit...");
                        _device.Set(setInhibit);
                    }

                    continue;
                }

                Console.WriteLine($"Status reached: {status}");

                if (status != JCMStatusResponse.Escrow)
                {
                    Console.WriteLine("Waiting for Escrow...");
                    if (!WaitForStatus(getStatus, out status, EscrowTimeout, JCMStatusResponse.Escrow))
                    {
                        Console.WriteLine("Timed out waiting for Escrow.");
                        continue;
                    }
                }
                Thread.Sleep(1000);
                Console.WriteLine("Escrowed — sending Stack1...");
                _device.Execute(stack1);

                if (!WaitForStatus(getStatus, out status, VendValidTimeout, JCMStatusResponse.VendValid))
                {
                    Console.WriteLine("Timed out waiting for VendValid.");
                    continue;
                }

                Console.WriteLine($"VendValid received at {DateTime.Now:HH:mm:ss.fff} — sending ACK.");
                _device.Execute(ack);
                Thread.Sleep(500);
                Console.WriteLine($"ACK sent at {DateTime.Now:HH:mm:ss.fff}");
                _device.Get(getStatus);
                if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.Stacked)
                {
                    Console.WriteLine($"[JCMDeviceAdapter] [Poll] Stacked r ");
                    Thread.Sleep(500);
                }
                else if (getStatus.OutputBuffer[0] == (byte)JCMStatusResponse.VendValid)
                {
                    _device.Execute(ack);
                    Console.Write("Acknowledge resent\n");
                }
                else if (getStatus.OutputBuffer[0] != (byte)JCMStatusResponse.Enable)
                {
                    Console.Write("Waiting for idling status. Current status 0x{0:X2}\n", getStatus.OutputBuffer[0]);
                    Thread.Sleep(500);
                }
                sw.Stop();
                Console.WriteLine("JCMDeviceAdapter Poll() Bill stacked successfully.");
                printTime(sw.ElapsedTicks, 1);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"JCMDeviceAdapter Poll() Exception: {ex.Message}");
            }
        }

        Console.WriteLine("JCMDeviceAdapter Poll() Polling ended.");
    }

    private void enableInterrupts(RAVDevice device)
    {

        if (device.IsNotificationFromDeviceEnabled)
        {
            Console.WriteLine("Interrupts already enabled in this device");
            return;
        }

        try
        {
            device.NotificationFromDeviceReceived += device_NotificationFromDeviceReceived;
            device.ToggleNotificationsFromDevice();
        }
        catch (Exception exc)
        {
            Console.WriteLine("Failed to enable device interrupts: " + exc.Message);
            return;
        }

        Console.WriteLine("Interrupts from device enabled");
    }

    void device_NotificationFromDeviceReceived(byte[] messageFromDevice)
    {
        Console.Write("\n");
        Console.Write("--------------- Interrupt data --------------------------\n\n");
        Console.Write("Received data length: {0}\n", messageFromDevice.Length);
        Console.Write("Buffer contents = { ");
        for (int i = 0; i < messageFromDevice.Length; i++)
        {
            Console.Write("0x{0:X2}", messageFromDevice[i]);
            if (i + 1 < messageFromDevice.Length)
                Console.Write(", ");
        }
        Console.Write(" }\n\n");
        Console.Write("------------ End of interrupt data -----------------------\n\n");
    }

    private bool WaitForStatus(JCMCommand getStatus, out JCMStatusResponse finalStatus, int maxRetries, params JCMStatusResponse[] expectedStatuses)
    {
        finalStatus = JCMStatusResponse.Failure;

        for (int retries = 0; retries < maxRetries && IsPolling; retries++)
        {
            _device.Get(getStatus);
            byte code = getStatus.OutputBuffer[0];
            finalStatus = (JCMStatusResponse)code;

            Console.WriteLine($"Polling: {finalStatus} (0x{code:X2})");
            if (finalStatus == JCMStatusResponse.Escrow)
            {
                byte[] buffer = getStatus.OutputBuffer;

                if (IsBillChannel(buffer[1]))
                {
                    int? denomIndex = GetDenominationIndex(buffer);
                    int mappedValue = MapChannelToValue(buffer[1]);
                    Console.WriteLine($"💶 Money inserted — Denomination index: {denomIndex} Mapped value: {mappedValue}€");
                }
                else
                {
                    string coupon = System.Text.Encoding.ASCII.GetString(buffer, 4, buffer.Length - 4).Trim('\0');
                    Console.WriteLine($"📠 Coupon detected: {coupon}");
                }

                Console.WriteLine("Escrow buffer: " + BitConverter.ToString(buffer));
            }

            if (expectedStatuses.Contains(finalStatus))
                return true;

            Thread.Sleep(_threadSleepTime);
        }

        return false;
    }
    private bool IsBillChannel(byte channel)
    {
        // Euro bill channels: 0x61-0x65
        return channel >= 61 && channel <= 65;
    }

    private int MapChannelToValue(byte channel)
    {
        // Euro bill mapping
        switch (channel)
        {
            case 0x61: return 0;   // €0
            case 0x62: return 5;   // €5
            case 0x63: return 10;  // €10
            case 0x64: return 20;  // €20
            case 0x65: return 50;  // €50

            default: return channel;
        }
    }

    private static bool IsCoupon(byte[] buffer)
    {
        if (buffer.Length < 16)
            return false;

        string ascii = System.Text.Encoding.ASCII.GetString(buffer, 4, buffer.Length - 4);
        return ascii.Any(c => char.IsLetterOrDigit(c)); // contains readable ASCII
    }

    private static int? GetDenominationIndex(byte[] buffer)
    {
        if (buffer.Length < 4)
            return null;

        byte raw = buffer[3];
        int index = (raw & 0x38) >> 3;

        return index;
    }

    public void ReturnBill()
    {
        this._device.ExecuteWithMenuOption(MenuOption.Return);
    }

    public void StackBill()
    {
        this._device.ExecuteWithMenuOption(MenuOption.Stack1);
    }

    public void Dispose() => _device.Dispose();

    private static void printTime(long ticks, int repetitions)
    {
        double seconds = ((double)ticks) / ((double)Stopwatch.Frequency);
        double avg = seconds / repetitions;

        Console.Write("\n----------------  Results  ----------------\n\n");
        Console.Write("Total execution time: ");

        if (seconds > 1)
            Console.WriteLine("{0:f4}s", seconds);
        else if (seconds * 1000 > 1)
            Console.WriteLine("{0:f4}ms", seconds * 1000);
        else
            Console.WriteLine("{0:f4}us", seconds * 1000 * 1000);

        Console.Write("Average cycle execution time: ");

        if (avg > 1)
            Console.WriteLine("{0:f4}s", avg);
        else if (avg * 1000 > 1)
            Console.WriteLine("{0:f4}ms", avg * 1000);
        else
            Console.WriteLine("{0:f4}us", avg * 1000 * 1000);

        Console.Write("\n----------------  End of results  ----------------\n");
    }
}