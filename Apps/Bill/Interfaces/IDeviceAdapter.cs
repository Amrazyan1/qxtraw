using Quixant.LibRAV;

public interface IDeviceAdapter : IDisposable
{
    bool IsOpen { get; }

    bool IsPolling { get; set; }


    SerialPortIndex port { get; set; }
    void Open();
    void Init();
    void Poll();
    void ReturnBill();
    void StackBill();
    event Action<String> OnStacked;
    event Action<String> OnReturned;
    event Action<String> OnEscrowed;
    event Action<String> OnAccepted;
    event Action<String> OnIdling;
    event Action<String> OnJammed;
    event Action<String> OnCheated;
    event Action<String> OnRejected;
    event Action<String> OnStackerFull;
    event Action<String> OnPaused;
    event Action<String> OnCalibration;
    event Action<String> OnPowerUp;
    event Action<String> OnInvalidCommand;
    event Action<String> OnFailure;
    // void Run(IDeviceCommand command);
    // void Execute(IDeviceCommand command);
    // void ExecuteWithMenuOption(MenuOption option);
    // void Set(IDeviceCommand command);
    // void Set(IDeviceExtendedCommand command);
    // uint Get(IDeviceCommand command);
}
