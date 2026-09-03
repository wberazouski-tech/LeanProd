namespace LeanProd.Application.Common.Abstractions;

public interface IRuntimeDatabaseConnection
{
    bool IsConfigured { get; }
    string Provider { get; }
    string ConnectionString { get; }
}
