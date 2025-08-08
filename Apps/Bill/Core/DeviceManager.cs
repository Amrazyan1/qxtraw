using System.Diagnostics;
using Quixant.LibRAV;

public class DeviceManager : IDisposable
{

    private readonly SerialPortIndex _defaultPort = SerialPortIndex.SerialPort4;
    private IDeviceAdapter _currentDeviceAdapter;

    public DeviceManager(Func<SerialPortIndex, IDeviceAdapter> adapterFactory)
    {
        _currentDeviceAdapter = adapterFactory(_defaultPort);
    }

    public event Action<string> OnStacked
    {
        add { _currentDeviceAdapter.OnStacked += value; }
        remove { _currentDeviceAdapter.OnStacked -= value; }
    }
    public event Action<string> OnReturned
    {
        add { _currentDeviceAdapter.OnReturned += value; }
        remove { _currentDeviceAdapter.OnReturned -= value; }
    }
    public event Action<string> OnEscrowed
    {
        add { _currentDeviceAdapter.OnEscrowed += value; }
        remove { _currentDeviceAdapter.OnEscrowed -= value; }
    }
    public event Action<string> OnAccepted
    {
        add { _currentDeviceAdapter.OnAccepted += value; }
        remove { _currentDeviceAdapter.OnAccepted -= value; }
    }
    public event Action<string> OnIdling
    {
        add { _currentDeviceAdapter.OnIdling += value; }
        remove { _currentDeviceAdapter.OnIdling -= value; }
    }
    public event Action<string> OnJammed
    {
        add { _currentDeviceAdapter.OnJammed += value; }
        remove { _currentDeviceAdapter.OnJammed -= value; }
    }
    public event Action<string> OnCheated
    {
        add { _currentDeviceAdapter.OnCheated += value; }
        remove { _currentDeviceAdapter.OnCheated -= value; }
    }
    public event Action<string> OnRejected
    {
        add { _currentDeviceAdapter.OnRejected += value; }
        remove { _currentDeviceAdapter.OnRejected -= value; }
    }
    public event Action<string> OnStackerFull
    {
        add { _currentDeviceAdapter.OnStackerFull += value; }
        remove { _currentDeviceAdapter.OnStackerFull -= value; }
    }
    public event Action<string> OnPaused
    {
        add { _currentDeviceAdapter.OnPaused += value; }
        remove { _currentDeviceAdapter.OnPaused -= value; }
    }
    public event Action<string> OnCalibration
    {
        add { _currentDeviceAdapter.OnCalibration += value; }
        remove { _currentDeviceAdapter.OnCalibration -= value; }
    }
    public event Action<string> OnPowerUp
    {
        add { _currentDeviceAdapter.OnPowerUp += value; }
        remove { _currentDeviceAdapter.OnPowerUp -= value; }
    }
    public event Action<string> OnInvalidCommand
    {
        add { _currentDeviceAdapter.OnInvalidCommand += value; }
        remove { _currentDeviceAdapter.OnInvalidCommand -= value; }
    }
    public event Action<string> OnFailure
    {
        add { _currentDeviceAdapter.OnFailure += value; }
        remove { _currentDeviceAdapter.OnFailure -= value; }
    }
    public event Action<string> OnCasseteRemoved
    {
        add { _currentDeviceAdapter.OnCasseteRemoved += value; }
        remove { _currentDeviceAdapter.OnCasseteRemoved -= value; }
    }

    public void Initalize()
    {
        _currentDeviceAdapter.Open();
        Console.WriteLine($"DeviceManager Initalize() Port opened on {_defaultPort.Name}.");
        _currentDeviceAdapter.Init();
        Console.WriteLine($"DeviceManager Initalize() Device on port {_defaultPort.Name} initialized. \n Polling Started.");

        _currentDeviceAdapter.Poll();
    }

    public void StopPolling()
    {
        _currentDeviceAdapter.IsPolling = false;
        Console.WriteLine("DeviceManager StopPolling() IsPolling false.");
    }

    public void StartPolling()
    {
        _currentDeviceAdapter.IsPolling = true;
        Console.WriteLine("DeviceManager StartPolling() IsPolling true., Starting poll.");
        _currentDeviceAdapter.Poll();
    }

    public void ReturnBill()
    {
        _currentDeviceAdapter.ReturnBill();
    }

    public void StackBill()
    {
        _currentDeviceAdapter.StackBill();
    }

    public void ClosePort()
    {
        try
        {
            Console.WriteLine($"DeviceManager ClosePort() closing port ....");
            _currentDeviceAdapter.IsPolling = false;
            _currentDeviceAdapter.Dispose();
            Console.WriteLine($"DeviceManager ClosePort() Device on port closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeviceManager ClosePort() ❌ Error closing device: {ex.Message}");
        }
    }

    public void Dispose()
    {
        ClosePort();
    }
}
