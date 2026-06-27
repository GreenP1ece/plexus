using System.Collections.Concurrent;
using System.Reflection;

namespace Common.Domain.Models;
public abstract class Enumeration(int value, string name) : IComparable
{
    private static readonly ConcurrentDictionary<Type, IEnumerable<object>> _enumCache = new();

    public int Value { get; } = value;

    public string Name { get; } = name;

    public int CompareTo(object? other) => Value.CompareTo(((Enumeration)other!).Value);

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var type = typeof(T);

        var values = _enumCache.GetOrAdd(type, _ => type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null)!));

        return values.Cast<T>();
    }

    public static T FromValue<T>(int value) where T : Enumeration
        => Parse<T, int>(value, nameof(value), item => item.Value == value);

    public static T FromName<T>(string name) where T : Enumeration
        => Parse<T, string>(name, nameof(name), item => item.Name == name);

    public static string NameFromValue<T>(int value) where T : Enumeration
        => FromValue<T>(value).Name;

    public static bool HasValue<T>(int value) where T : Enumeration
    {
        try
        {
            FromValue<T>(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    public override string ToString() => Name;

    public override int GetHashCode() => (GetType().ToString() + Value).GetHashCode();

    public static bool operator ==(Enumeration? first, Enumeration? second)
    {
        if (first is null && second is null)
        {
            return true;
        }

        if (first is null || second is null)
        {
            return false;
        }

        return first.Equals(second);
    }

    public static bool operator !=(Enumeration? first, Enumeration? second) => !(first == second);

    private static T Parse<T, TValue>(TValue value, string description, Func<T, bool> predicate)
        where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
        {
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }

        return matchingItem;
    }
}