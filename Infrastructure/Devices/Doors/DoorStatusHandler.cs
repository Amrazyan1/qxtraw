using Quixant.Core;

class DoorStatusHandler
{

    public void Poll()
    {
        Console.Write("Initializing core...");
        CoreManager core = CoreManager.GetDefault();
        Console.WriteLine("done\n");

        core.LoggingProcessor.DoorStateChanged += (sender, e) =>
        {
            Console.WriteLine($"\n >> Intrusion door #{e.DoorIndex} {e.State} ({e.EventsInLog} events in log)");
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

        Console.WriteLine("Polling intrusion doors state every 5 seconds. Press ENTER or ^C to exit.");

        ThreadPool.QueueUserWorkItem(_ =>
        {
            while (true)
            {
                var ds = core.LoggingProcessor.IntrusionDoors.DoorStatus;
                ds.Refresh();
                Console.WriteLine($"[{DateTime.Now.ToString()}] #0: {ds[0]}, #1: {ds[1]},  #2: {ds[2]},  #3: {ds[3]},  #4: {ds[4]},  #5: {ds[5]},  #6: {ds[6]},  #7: {ds[7]}");
                Thread.Sleep(5000);
            }
        });
    }
}
