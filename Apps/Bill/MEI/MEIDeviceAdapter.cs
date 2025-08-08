using System.Diagnostics;
using Quixant.LibRAV;

public class MEIDeviceAdapter : IDeviceAdapter
{
    private readonly RAVDevice _device;

    public MEIDeviceAdapter(SerialPortIndex port)
    {
        _device = new RAVDevice(port, ProtocolIdentifier.MEI);
        _port = port;
    }


    public bool IsOpen => _device.IsOpen;

    public bool _isPolling = true;


    private SerialPortIndex _port;

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
    public event Action<string> OnCasseteRemoved;

    public SerialPortIndex port { get => _port; set => _port = value; }
    public bool IsPolling { get => _isPolling; set => _isPolling = value; }

    public void Open()
    {
        if (_device.IsOpen)
        {
            Console.WriteLine($"Device on port {port.Name} already open.");
            return;
        }
        Console.WriteLine($"Initializing {_device.Protocol} _device on port {port.Name}...");

        try
        {
            _device.Open(port.Name);

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error initializing _device: {ex.Message}");
        }
    }

    public void Init()
    {
        MEIInstruction instruction = MEIInstruction.InitAndPoll;//TODO change to Icommand adapter
        uint outLen = 0;
        MEICommand reset = new MEICommand(MEIInstruction.SoftReset, 0, 0);
        MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);
        MEICommand setDenom = new MEICommand(MEIInstruction.SetDenomination, 1, 0);
        MEICommand setInt = new MEICommand(MEIInstruction.SetSpecialInterruptMode, 1, 0);
        // MEICommand setSec = new MEICommand(MEIInstruction.SetSecurity, 1, 0);
        MEICommand setOri = new MEICommand(MEIInstruction.SetOrientation, 2, 0);
        MEICommand setEscrow = new MEICommand(MEIInstruction.SetEscrowMode, 1, 0);
        MEICommand setPush = new MEICommand(MEIInstruction.SetPushMode, 1, 0);
        MEICommand setBar = new MEICommand(MEIInstruction.SetBarcodeDecoding, 1, 0);
        MEICommand setPup = new MEICommand(MEIInstruction.SetPowerup, 2, 0);
        MEICommand setNote = new MEICommand(MEIInstruction.SetExtendedNoteReporting, 1, 0);
        MEICommand setCpn = new MEICommand(MEIInstruction.SetExtendedCouponReporting, 1, 0);
        setDenom.InputBuffer[0] = 0x7f;
        setInt.InputBuffer[0] = 0x00;
        // setSec.InputBuffer[0] = 0x00;
        setOri.InputBuffer[0] = 0x03;
        setOri.InputBuffer[1] = 0x00;
        setEscrow.InputBuffer[0] = 0x01;
        setPush.InputBuffer[0] = 0x00;
        setBar.InputBuffer[0] = 0x01;
        setPup.InputBuffer[0] = 0x00;
        setPup.InputBuffer[1] = 0x00;
        setCpn.InputBuffer[0] = 0x01;

        try
        {
            int initWait = 0;

            _device.Execute(reset);
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Waiting for the _device to initialize...");

            while (initWait < 30)
            {
                try
                {
                    outLen = _device.Get(stdHostToAcc);
                    Console.WriteLine("[MEIDeviceAdapter] initMEI() Initialization done");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MEIDeviceAdapter] initMEI() initWait. {ex.Message}");
                    initWait++;
                    Thread.Sleep(50);
                }
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Operation failed: " + exc.Message);
            return;
        }
        Stopwatch sw = null;
        sw = Stopwatch.StartNew();

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() _device.Set(setDenom);");
            _device.Set(setDenom);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set denomination failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() _device.Set(setInt);");
            _device.Set(setInt);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set interrupt failed: " + exc.Message);
            return;
        }

        // try
        // {
        //     _device.Set(setSec);
        // }
        // catch (Exception exc)
        // {
        //     Console.WriteLine("MEIDeviceAdapter initMEI() Set security failed: " + exc.Message);
        //     return;
        // }

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() _device.Set(setOri);");

            _device.Set(setOri);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set orientation failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() setEscrow.RunOn(_device);");
            _device.Set(setEscrow);
            // setEscrow.RunOn(_device); //alternate way of calling a command
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set escrow failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() setPush.RunOn(_device);");
            _device.Set(setPush);
            // setPush.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set push failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() setBar.RunOn(_device);");
            _device.Set(setBar);
            // setBar.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set barcode decoding failed: " + exc.Message);
            return;
        }

        try
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() setPup.RunOn(_device);");
            _device.Set(setPup);
            // setPup.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Set powerup failed: " + exc.Message);
            return;
        }
        sw.Stop();
        printTime(sw.ElapsedTicks, 1);
        try
        {
            switch (instruction)
            {
                case MEIInstruction.InitAndPoll:   // Normal mode
                    {
                        // Enable extended note reporting
                        setNote.InputBuffer[0] = 0x00;
                        setNote.RunOn(_device);

                        break;
                    }
                case MEIInstruction.InitExtCfscAndPoll:   // Extended Note CFSC - 8 bytes of denomination
                    {
                        // Enable extended note reporting 
                        setNote.InputBuffer[0] = 0x01;
                        setNote.RunOn(_device);

                        //Enable all the Bank Note
                        MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
                            (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);

                        for (int i = 0; i < 8; i++)
                            setExtendedNote.InputBuffer[i] = 0xFF;
                        _device.Set(setExtendedNote);
                        break;
                    }
                case MEIInstruction.InitExtScaScrAndPoll:   // Extended Note SC Adv SCR - 19 bytes of denomination
                    {
                        // Enable extended note reporting
                        setNote.InputBuffer[0] = 0x02;
                        setNote.RunOn(_device);

                        //Enable all the Bank Note
                        MEIExtendedCommand setExtendedNote = new MEIExtendedCommand
                            (MEIMessageExtendedSubtype.SetExtendedNoteInhibits, 8);
                        _device.Set(setExtendedNote);

                        break;
                    }

                default:
                    break;
            }
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Init and Poll failed: " + exc.Message);
            return;
        }
        try
        {
            setCpn.RunOn(_device);
        }
        catch (Exception exc)
        {
            Console.WriteLine("[MEIDeviceAdapter] initMEI() Disable extended coupon reporting failed: " + exc.Message);
            return;
        }

        Console.Write("[MEIDeviceAdapter] initMEI() Test executed successfully\n");
    }

    public void Poll()
    {
        uint outLen = 0;
        MEICommand stdHostToAcc = new MEICommand(MEIInstruction.StdHostToAcc, 0, 128);

        while (IsPolling)
        {
            try
            {
                outLen = stdHostToAcc.RunOn(_device);// _device.Get(stdHostToAcc);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[MEIDeviceAdapter] Poll() ERROR {ex.Message}");
                continue;
            }

            uint status = BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 1);
            MeiStatus parsedStatus = (MeiStatus)status;
            DispatchStatusEvents(parsedStatus);
            if (parsedStatus.HasFlag(MeiStatus.Rejected))
            {
                Console.WriteLine("[MEIDeviceAdapter] !IMPORTANT Bill rejected. Reason code: 0x{0:X2}", stdHostToAcc.OutputBuffer[4]);
            }

            if (!parsedStatus.HasFlag(MeiStatus.CassetteAttached))
            {
                Console.WriteLine("[MEIDeviceAdapter] !IMPORTANT CassetteRemoved or missing}");
                OnCasseteRemoved?.Invoke("[MEIDeviceAdapter] Cassette removed or missing");
            }

            Console.WriteLine($"[MEIDeviceAdapter] Polling status: 0x{status:X8} outlen : {outLen}, outpurBufferLenght:{stdHostToAcc.OutputBuffer.Length} {parsedStatus}");

            if ((stdHostToAcc.OutputBuffer[0] & 0xF0) == (int)MEIInstruction.ExtendedMsgSet)
            {
                byte subtype = stdHostToAcc.OutputBuffer[1];
                if (subtype == (byte)MEIMessageExtendedSubtype.ExtendedBarcodeReply)
                {
                    // The extended data field for Barcodes is 28 bytes long and represented in ASCII.
                    // The Barcode data is left justified LSC (Least Significant Character)
                    // and all unused bytes are filled with 0x28.
                    // First 8 bytes are:
                    // Message type + Sybtype + Status data (4 bytes) + Model# + Revision#
                    Console.Write("Barcode value: ");
                    for (int i = 8; i < 28 && stdHostToAcc.OutputBuffer[i] != 0x28; i++)
                    {
                        Console.Write((char)stdHostToAcc.OutputBuffer[i]);
                    }
                    Console.WriteLine();
                    Console.WriteLine("Sending stack command...");
                    Thread.Sleep(1000);
                    StackBill();
                }
            }
            else if ((stdHostToAcc.OutputBuffer[0] & 0xF0) == (int)MEIInstruction.StdAccToHost)
            {
                if (outLen >= 5 && parsedStatus.HasFlag(MeiStatus.Escrowed))
                {
                    Console.WriteLine("[MEIDeviceAdapter] MeiPoll() Received escrowed event");
                    int denominationIndex = (stdHostToAcc.OutputBuffer[3] & 0x38) >> 3;
                    Console.WriteLine($"[MEIDeviceAdapter] index: {denominationIndex}");
                    Thread.Sleep(1000);
                    OnEscrowed?.Invoke($"[MEIDeviceAdapter] Escrowed: {stdHostToAcc.OutputBuffer[1]}");
                    StackBill();
                    WaitForSignalsAfterStacking(stdHostToAcc, parsedStatus);
                }
                else if (outLen >= 10 && (((MeiStatus)BitConverter.ToUInt32(stdHostToAcc.OutputBuffer, 2)) & MeiStatus.Escrowed) == MeiStatus.Escrowed)
                {
                    Console.WriteLine("[MEIDeviceAdapter] MeiPoll() Received status Extended : 0x{0:X2} 0x{1:X2} 0x{2:X2}", stdHostToAcc.OutputBuffer[0], stdHostToAcc.OutputBuffer[1], stdHostToAcc.OutputBuffer[2]);
                    Console.WriteLine("[MEIDeviceAdapter] MeiPoll() Received escrowed event");
                    int denominationIndex = (stdHostToAcc.OutputBuffer[3] & 0x38) >> 3;
                    Console.WriteLine($"[MEIDeviceAdapter] Denomination index: {denominationIndex}");
                    Thread.Sleep(1000);
                    OnEscrowed?.Invoke($"[MEIDeviceAdapter] Escrowed: {stdHostToAcc.OutputBuffer[1]}");
                    StackBill();
                    WaitForSignalsAfterStacking(stdHostToAcc, parsedStatus);
                }
            }

            Thread.Sleep(250);
        }

        Console.WriteLine("[MEIDeviceAdapter] Devicemanager MeiPoll() exited polling loop.");
    }
    private void DispatchStatusEvents(MeiStatus status)
    {
        if (status.HasFlag(MeiStatus.Idling)) OnIdling?.Invoke("[MEIDeviceAdapter] Status: Idling");
        if (status.HasFlag(MeiStatus.Accepting)) OnAccepted?.Invoke("[MEIDeviceAdapter] Status: Accepting");
        if (status.HasFlag(MeiStatus.Escrowed)) OnEscrowed?.Invoke("[MEIDeviceAdapter] Status: Escrowed");
        if (status.HasFlag(MeiStatus.Stacking)) OnStacked?.Invoke("[MEIDeviceAdapter] Status: Stacking");
        if (status.HasFlag(MeiStatus.Stacked)) OnStacked?.Invoke("[MEIDeviceAdapter] Status: Stacked");
        if (status.HasFlag(MeiStatus.Returning)) OnReturned?.Invoke("[MEIDeviceAdapter] Status: Returning");
        if (status.HasFlag(MeiStatus.Returned)) OnReturned?.Invoke("[MEIDeviceAdapter] Status: Returned");
        if (status.HasFlag(MeiStatus.Cheated)) OnCheated?.Invoke("[MEIDeviceAdapter] Status: Cheated");
        if (status.HasFlag(MeiStatus.Rejected)) OnRejected?.Invoke("[MEIDeviceAdapter] Status: Rejected");
        if (status.HasFlag(MeiStatus.Jammed)) OnJammed?.Invoke("[MEIDeviceAdapter] Status: Jammed");
        if (status.HasFlag(MeiStatus.StackerFull)) OnStackerFull?.Invoke("[MEIDeviceAdapter] Status: Stacker Full");
        if (status.HasFlag(MeiStatus.Paused)) OnPaused?.Invoke("[MEIDeviceAdapter] Status: Paused");
        if (status.HasFlag(MeiStatus.Calibration)) OnCalibration?.Invoke("[MEIDeviceAdapter] Status: Calibration");
        if (status.HasFlag(MeiStatus.PowerUp)) OnPowerUp?.Invoke("[MEIDeviceAdapter] Status: Power Up");
        if (status.HasFlag(MeiStatus.InvalidCommand)) OnInvalidCommand?.Invoke("[MEIDeviceAdapter] Status: Invalid Command");
        if (status.HasFlag(MeiStatus.Failure)) OnFailure?.Invoke("[MEIDeviceAdapter] Status: Failure");

        // Special case: cassette removed
        if (!status.HasFlag(MeiStatus.CassetteAttached)) OnCasseteRemoved?.Invoke("[MEIDeviceAdapter] Cassette removed or missing");
    }

    private void WaitForSignalsAfterStacking(MEICommand stdHostToAcc, MeiStatus parsedStatus)
    {
        uint outLen = _device.Get(stdHostToAcc);
        if (outLen >= 5 && BitConverter.ToUInt16(stdHostToAcc.OutputBuffer, 1) != 0x1001)
        {
            Console.WriteLine("[MEIDeviceAdapter] MeiPoll() Received status: 0x{0:X8}", stdHostToAcc.OutputBuffer[1]);
        }

        if (outLen >= 5 && parsedStatus.HasFlag(MeiStatus.Stacked))
        {
            Console.WriteLine("[MEIDeviceAdapter] [MeiPoll()] Stacked outLen[5]", stdHostToAcc.OutputBuffer[1]);
            OnStacked?.Invoke($"[MEIDeviceAdapter] Stacked: {stdHostToAcc.OutputBuffer[1]}");
        }
        if (outLen >= 10 && parsedStatus.HasFlag(MeiStatus.Stacked))
        {
            Console.WriteLine("[MEIDeviceAdapter] [MeiPoll()] Stacked [outLen10]", stdHostToAcc.OutputBuffer[2]);
            OnStacked?.Invoke($"[MEIDeviceAdapter] Stacked: {stdHostToAcc.OutputBuffer[2]}");
        }

    }

    public void ReturnBill()
    {
        Console.WriteLine("[MEIDeviceAdapter] ReturnBill() Bill return.");
        this._device.ExecuteWithMenuOption(MenuOption.MEI_Return);
    }

    public void StackBill()
    {
        Console.WriteLine("[MEIDeviceAdapter] StackBill() ");
        this._device.ExecuteWithMenuOption(MenuOption.MEI_Stack);
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
