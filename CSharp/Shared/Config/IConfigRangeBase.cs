namespace ModdingToolkit.Config;

public interface IConfigRangeBase<T> : IConfigEntry<T> where T: IConvertible
{
    public T MinValue { get; }
    public T MaxValue { get; }
    public T StepAmount { get; }
}