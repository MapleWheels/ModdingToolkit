namespace ModdingToolkit;

public static class MemoryCallbackCache
{
    private static Dictionary<Guid, CallbackMachine> CallbackMachines = new();


    public static Guid CreateInstance()
    {
        Guid id = Guid.NewGuid();
        CallbackMachines.Add(id, new CallbackMachine(id));
        return id;
    }

    public static bool DisposeInstance(Guid instanceId)
    {
        if (!CallbackMachines.ContainsKey(instanceId))
            return false;
        CallbackMachines[instanceId].RemoveAll();
        CallbackMachines.Remove(instanceId);
        return true;
    }
    
    public static Guid AddCallback<T1, T2>(Guid id, T1 target, T2 value, Action<WeakReference<T1>, T2> callback) 
        where T1 : class where T2 : IConvertible
    {
        if (!CallbackMachines.ContainsKey(id))
            return Guid.Empty;
        return CallbackMachines[id].RegisterNewCallback(new WeakReference<T1>(target), value, callback);
    }

    public static bool Invoke(Guid instanceId, Guid callbackId)
    {
        if (!CallbackMachines.ContainsKey(instanceId))
            return false;
        return CallbackMachines[instanceId].ExecuteCallback(callbackId);
    }

    public static bool Remove(Guid instanceId, Guid callbackId)
    {
        if (!CallbackMachines.ContainsKey(instanceId))
            return false;
        return CallbackMachines[instanceId].RemoveCallback(callbackId);
    }

    public static bool ExecuteAndRemove(Guid instanceId, Guid callbackId)
    {
        if (!CallbackMachines.ContainsKey(instanceId))
            return false;
        if (CallbackMachines[instanceId].ExecuteCallback(callbackId))
            return CallbackMachines[instanceId].RemoveCallback(callbackId);
        return false;
    }

    public static bool ExecuteAll(Guid instanceId)
    {
        if (!CallbackMachines.ContainsKey(instanceId))
            return false;
        CallbackMachines[instanceId].ExecuteAll();
        return true;
    }

    public static bool RemoveAll(Guid instanceId)
    {
        if (!CallbackMachines.ContainsKey(instanceId))
            return false;
        CallbackMachines[instanceId].RemoveAll();
        return true;
    }

    public static bool ExecuteAndRemoveAll(Guid instanceId)
    {
        if (ExecuteAll(instanceId))
            return RemoveAll(instanceId);
        return false;
    }


    private class CallbackMachine
    {
        public readonly Guid Id;
        public readonly Dictionary<Guid, CallbackRef> Callbacks;
        
        public CallbackMachine(Guid id)
        {
            this.Id = id;
            this.Callbacks = new();
        }

        public Guid RegisterNewCallback<T1, T2>(WeakReference<T1> target, T2 value,
            Action<WeakReference<T1>, T2> callback) where T1 : class where T2 : IConvertible
        {
            Guid callbackId = Guid.NewGuid();
            CacheStore<T1,T2>.CallbackStores.Add(
                callbackId, 
                new CacheStore<T1, T2>.CallbackStore(target, value, callback));
            this.Callbacks.Add(callbackId, new CallbackRef(
                () => 
                {
                    var c = CacheStore<T1, T2>.CallbackStores[callbackId];
                    c.Callback?.Invoke(c.Target, c.Value);
                },
                () => CacheStore<T1, T2>.CallbackStores.Remove(callbackId)));
            return callbackId;
        }

        public bool ExecuteCallback(Guid callbackId)
        {
            if (!this.Callbacks.ContainsKey(callbackId))
                return false;
            this.Callbacks[callbackId].Execute?.Invoke();
            return true;
        }

        public bool RemoveCallback(Guid callbackId)
        {
            if (!this.Callbacks.ContainsKey(callbackId))
                return false;
            this.Callbacks[callbackId].Remove?.Invoke();
            this.Callbacks.Remove(callbackId);
            return true;
        }

        public void ExecuteAll()
        {
            foreach (var pair in Callbacks)
                pair.Value.Execute?.Invoke();
        }

        public void RemoveAll()
        {
            foreach (var pair in Callbacks)
                pair.Value.Remove?.Invoke();
            Callbacks.Clear();
        }

        public record CallbackRef(Action Execute, Action Remove);
    }

    private static class CacheStore<T1, T2> where T1 : class where T2 : IConvertible
    {
        public static readonly Dictionary<Guid, CallbackStore> CallbackStores = new();
        public record CallbackStore(WeakReference<T1> Target, T2 Value, Action<WeakReference<T1>, T2> Callback);
    }
}