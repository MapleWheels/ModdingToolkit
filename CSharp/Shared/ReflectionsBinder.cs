using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Linq;
using Barotrauma;
using HarmonyLib;

namespace ModConfigManager;

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
    public object? ObjInstance { get; protected set; }
    public string? Name { get; protected set; }
    protected MemberType MType;
    protected MemberInfo refMember;

    public TVar? Value
    {
        get => GetValue(this.ObjInstance);
        set => SetValue(value);
    }

    public Bindable(string? name = null)
    {
        if (name is not null)
            this.Name = name;
    }

    public void Bind(object? instance, string? name = null, bool isInit = false)
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
        
        this.ObjInstance = instance;

        BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                             BindingFlags.FlattenHierarchy;
        
        FieldInfo? fi = typeof(TObj).GetField(this.Name);
        if (fi is null)
        {
            PropertyInfo? pi = typeof(TObj).GetProperty(this.Name);
            this.refMember = pi ?? throw new ArgumentException($"Binder::Bind() | Could not find a member named {this.Name}.");
            this.MType = MemberType.Property;
        }
        else
        {
            this.refMember = fi;
            this.MType = MemberType.Field;
        }
    }
    
    public void SetValue(TVar? value)
    {
        if (this.refMember is null)
            throw new NullReferenceException($"Bindable::SetValue() | Reference member is null. Did you call Bind()?");
        
        switch (this.MType)
        {
            case MemberType.Field: ((FieldInfo)this.refMember).SetValue(this.ObjInstance, value);
                break;
            case MemberType.Property: ((PropertyInfo)this.refMember).SetValue(this.ObjInstance, value);
                break;
        }
    }

    public TVar? GetValue(object? instance)
    {
        if (this.refMember is null)
            throw new NullReferenceException($"Bindable::SetValue() | Reference member is null. Did you call Bind()?");
        return _GetValue(instance);
    }
    
    private TVar? _GetValue(object? instance) =>
        this.MType switch
        {
            MemberType.Field => (TVar?)((FieldInfo)this.refMember).GetValue(instance ?? this.ObjInstance),
            MemberType.Property => (TVar?)((PropertyInfo)this.refMember).GetValue(instance ?? this.ObjInstance)
        };
    
    public static implicit operator TVar?(Bindable<TObj, TVar> obj) => obj.Value;
    
    public enum MemberType
    {
        Field,
        Property
    }
}


