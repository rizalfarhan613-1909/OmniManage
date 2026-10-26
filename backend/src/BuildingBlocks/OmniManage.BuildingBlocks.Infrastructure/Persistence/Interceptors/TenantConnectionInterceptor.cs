using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OmniManage.BuildingBlocks.Application;

namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Interceptors;

public sealed class TenantConnectionInterceptor(ITenantContext tenantContext) : DbConnectionInterceptor
{
    private const string SetTenantCommand = "SELECT set_config('app.tenant_id', @tenant_id, false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(connection);

        SetTenant(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        await SetTenantAsync(connection, cancellationToken).ConfigureAwait(false);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken).ConfigureAwait(false);
    }

    private void SetTenant(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = SetTenantCommand;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "tenant_id";
        parameter.Value = tenantContext.TenantId.ToString("D", CultureInfo.InvariantCulture);
        command.Parameters.Add(parameter);

        command.ExecuteNonQuery();
    }

    private async Task SetTenantAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = SetTenantCommand;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "tenant_id";
        parameter.Value = tenantContext.TenantId.ToString("D", CultureInfo.InvariantCulture);
        command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
