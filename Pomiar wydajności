using System;
using System.Diagnostics;

public class PerformanceMeasurer
{
    public static (long memoryMB, TimeSpan time) Measure(Action action)
    {
        var sw = Stopwatch.StartNew();
        long startMemory = GC.GetTotalMemory(true);
        
        action.Invoke();
        
        sw.Stop();
        long endMemory = GC.GetTotalMemory(true);
        return ((endMemory - startMemory) / 1024 / 1024, sw.Elapsed);
    }
}
