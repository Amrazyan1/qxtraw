using Quixant.Core;

Console.WriteLine("Quixant.LibQxt example - lp_intrusions");

Console.Write("Initializing core...");
CoreManager core = CoreManager.GetDefault();
Console.WriteLine("done\n");

core.LoggingProcessor.DoorStateChanged += (sender, e) =>
{
    Console.WriteLine($"\n >> Intrusion door #{e.DoorIndex} {e.State} ({e.EventsInLog} events in log)");
};

Console.WriteLine("Configuring intrusion doors 0-7 to Normally Open...");
Console.WriteLine("WARNING: THIS SETUP WILL PERSIST UNTIL CHANGED AGAIN BY USER");

bool confirm = false, validAnswer = false;

while (!validAnswer)
{
    Console.Write("Do you want to continue? (y/n): ");
    string? answer = Console.ReadLine();

    if (answer == null)
        continue;

    if (answer.Equals("y", StringComparison.InvariantCultureIgnoreCase) || answer.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
    {
        confirm = true;
        validAnswer = true;
    }
    else if (answer.Equals("n", StringComparison.InvariantCultureIgnoreCase) || answer.Equals("no", StringComparison.InvariantCultureIgnoreCase))
    {
        confirm = false;
        validAnswer = true;
    }
}

if (!confirm)
{
    Console.WriteLine("Intrusion door configuration skipped.");
}
else
{
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
    Console.WriteLine("Intrusion door configuration completed.");
}

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

Console.ReadLine();

return 0;