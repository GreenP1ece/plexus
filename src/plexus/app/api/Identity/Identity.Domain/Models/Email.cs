using System.Text.RegularExpressions;
using Common.Domain.Models;

namespace Identity.Domain.Models;
public partial class Email : ValueObject
{
    private static readonly Regex _emailRegex = EmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        string trimmedEmail = email.Trim().ToLowerInvariant();

        if (!_emailRegex.IsMatch(trimmedEmail))
        {
            throw new FormatException($"Invalid email format: '{email}'.");
        }

        return new Email(trimmedEmail);
    }
    public static bool TryCreate(string email, out Email? result)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            result = default;
            return false;
        }

        string trimmedEmail = email.Trim().ToLowerInvariant();

        if (!_emailRegex.IsMatch(trimmedEmail))
        {
            result = default;
            return false;
        }

        result = new Email(trimmedEmail);
        return true;
    }

    public bool Equals(Email other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Email left, Email right) => left.Equals(right);
    public static bool operator !=(Email left, Email right) => !left.Equals(right);

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
