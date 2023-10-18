namespace ModdingToolkit;

public abstract class ReflectionsBinder<TTarget> where TTarget : class
{
    private static Harmony? harmonyInstance;
    private static readonly Dictionary<string, HarmonyMethod> prefixBinds = new();
    private static readonly Dictionary<string, HarmonyMethod> postfixBinds = new();
    private static readonly List<MethodInfo> origMethods = typeof(TTarget).GetMethods(
        BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public | BindingFlags.FlattenHierarchy |
        BindingFlags.NonPublic).ToList();

    protected ReflectionsBinder()
    {
        if (harmonyInstance == null)
            harmonyInstance = new Harmony(this.GetType().Name);

        this.GetType().GetMethods(BindingFlags.Static | 
                                  BindingFlags.FlattenHierarchy | 
                                  BindingFlags.NonPublic | BindingFlags.Public).ToList()
            .ForEach(m =>
            {
                if (m.Name.ToLowerInvariant().StartsWith("prefix_"))
                    prefixBinds.Add(m.Name.Substring(7), new HarmonyMethod(m));
                else if (m.Name.ToLowerInvariant().StartsWith("postfix_"))
                    postfixBinds.Add(m.Name.Substring(8), new HarmonyMethod(m));
            });
    }

    public void BindAll(object? instance)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                             BindingFlags.FlattenHierarchy;
        FieldInfo[] fields = this.GetType().GetFields(flags);
        PropertyInfo[] properties = this.GetType().GetProperties(flags);

        
        //assumes it has a type of "Bindable" which defines the method "Bind". This should be checked via
        //"GetGenericTypeDefinition" but that's too much of a hassle.
        
        foreach (FieldInfo field in fields)
        {
            field.FieldType.GetMethod("Bind")?.Invoke(this, new object?[]
            {
                instance, field.Name, true
            });
        }
        
        foreach (PropertyInfo property in properties)
        {
            property.PropertyType.GetMethod("Bind")?.Invoke(this, new object?[]
            {
                instance, property.Name, true
            });
        }
    }

    public void PatchAll()
    {
        origMethods.ForEach(m =>
        {
            HarmonyMethod? pre, post;
            if (!prefixBinds.TryGetValue(m.Name, out pre))
                pre = null;
            if (!postfixBinds.TryGetValue(m.Name, out post))
                post = null;
            harmonyInstance?.Patch(m, pre, post);
        });
    }

    public void UnPatchAll()
    {
        harmonyInstance?.UnpatchAll();
    }

    ~ReflectionsBinder()
    {
        this.UnPatchAll();
    }
}

public class Bindable<TObj, TVar> where TObj : class
{
    public WeakReference<TObj>? ObjRef { get; protected set; }
    public string? Name { get; protected set; }
    protected MemberType MType;
    protected MemberInfo? RefMember;
    protected bool IsInstanceType = false;

    public TVar? Value
    {
        get
        {
            if (this.RefMember is null)
                throw new NullReferenceException($"Bindable::SetValue() | Reference member is null. Did you call Bind()?");
            var weakReference = this.ObjRef;
            if (weakReference != null && this.IsInstanceType && weakReference.TryGetTarget(out var val))
            {
                switch (this.MType)
                {
                    case MemberType.Field: return (TVar?)((FieldInfo)this.RefMember).GetValue(this._GetRefObj());
                    case MemberType.Property: return (TVar?)((PropertyInfo)this.RefMember).GetValue(this._GetRefObj());
                    default: return default;
                }
            }
            return default;
        }
        set => SetValue(value);
    }

    public Bindable(string? name = null)
    {
        if (name is not null)
            this.Name = name;
    }

    public virtual bool IsValid
    {
        get
        {
            if (RefMember is null)
                return false;
            if (!IsInstanceType)
                return true;
            if (ObjRef is null)
                return false;
            return ObjRef.TryGetTarget(out _);
        }
    }

    public Bindable<TObj, TVar> Bind(object? instance, string? name = null, bool isInit = false)
    {
        if (this.Name.IsNullOrWhiteSpace())
        {
            if (isInit && !name.IsNullOrWhiteSpace())
                this.Name = name;
            else
                throw new ArgumentException($"Bindable::Bind() | Name cannot be null");
        }
        else if (!isInit && !name.IsNullOrWhiteSpace())
            this.Name = name;


        this.IsInstanceType = instance is not null;
        if (this.IsInstanceType)
            this.ObjRef = new WeakReference<TObj>((TObj)instance!, true);
        

        FieldInfo? fi = AccessTools.DeclaredField(typeof(TObj), this.Name);
        if (fi is null)
        {
            PropertyInfo? pi = AccessTools.DeclaredProperty(typeof(TObj), this.Name);
            this.RefMember = pi ?? throw new ArgumentException($"Binder::Bind() | Could not find a member named {this.Name}.");
            this.MType = MemberType.Property;
        }
        else
        {
            this.RefMember = fi;
            this.MType = MemberType.Field;
        }

        return this;
    }
    
    public void SetValue(TVar? value)
    {
        if (this.RefMember is null)
            throw new NullReferenceException($"Bindable::SetValue() | Reference member is null. Did you call Bind()?");
        
        switch (this.MType)
        {
            case MemberType.Field: ((FieldInfo)this.RefMember).SetValue(this._GetRefObj(), value);
                break;
            case MemberType.Property: ((PropertyInfo)this.RefMember).SetValue(this._GetRefObj(), value);
                break;
        }
    }

    private TObj? _GetRefObj()
    {
        var weakReference = this.ObjRef;
        return !IsInstanceType ? null :
            weakReference != null && weakReference.TryGetTarget(out var val) ? val : null;
    }

    public void Dispose()
    {
        this.ObjRef = null;
        this.RefMember = null;
    }

    public static implicit operator TVar(Bindable<TObj, TVar> obj) => obj.Value!;

    public enum MemberType
    {
        Field,
        Property
    }
}


