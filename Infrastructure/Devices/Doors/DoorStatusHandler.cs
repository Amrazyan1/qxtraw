using Quixant.Core;

class DoorStatusHandler
{
    public event Action<EnumDoors, DoorState> OnDoorStatusChangedEvent;
    public void Init()
    {
        Console.Write("Initializing core...");
        CoreManager core = CoreManager.GetDefault();
        Console.WriteLine("done\n");

        core.LoggingProcessor.DoorStateChanged += (sender, e) =>
        {
            Console.WriteLine($"\n >> [DoorStatusHandler] [DoorStateChanged()] Intrusion door #{e.DoorIndex} {e.State} ({e.EventsInLog} events in log)");
            OnDoorStatusChangedEvent?.Invoke((EnumDoors)e.DoorIndex, e.State);
        };

        core.LoggingProcessor.IntrusionDoors.Configure([
                  DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO,
        DoorSwitchConfiguration.NO
              ]);
    }
}
