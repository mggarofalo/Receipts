namespace Infrastructure.Entities;

public class ApiKeyEntity : IEquatable<ApiKeyEntity>
{
    public string ApiKeyHash { get; set; } = string.Empty;

    public bool Equals(ApiKeyEntity? other)
    {
        if (other is null)
        {
            return false;
        }

        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ApiKeyEntity)obj);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ApiKeyHash);
        return hash.ToHashCode();
    }

    public static bool operator ==(ApiKeyEntity? left, ApiKeyEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ApiKeyEntity? left, ApiKeyEntity? right)
    {
        return !Equals(left, right);
    }
}
