using System;
using System.Runtime;

public class GarbageCollectorConfig
{
    // Ustaw tryb Server GC (wymaga konfiguracji w runtimeconfig.json)
    public static void EnableServerGC()
    {
        AppContext.SetSwitch("System.GC.Server", true);
    }

    // Ogranicz maksymalną pamięć do 256 MB
    public static void SetHeapHardLimit()
    {
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;
        AppContext.SetData("GCHeapHardLimit", (ulong)256 * 1024 * 1024);
    }

    // Ręczne wywołanie GC z kompaktacją LOH
    public static void ManualGcCollect()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    // Przykład object poolingu
    public class ObjectPool<T> where T : new()
    {
        private readonly System.Collections.Concurrent.ConcurrentBag<T> _objects = new();

        public T Get() => _objects.TryTake(out T item) ? item : new T();
        public void Return(T item) => _objects.Add(item);
    }

    // Przykład użycia IDisposable
    public class ManagedResource : IDisposable
    {
        private byte[] _largeArray = new byte[1000000];
        public void Dispose() => _largeArray = null;
    }
}
