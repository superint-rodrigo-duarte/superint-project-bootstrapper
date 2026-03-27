using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class PostgresDatabaseService(ISshService sshService, DatabaseContainerEnvironmentSettings dtoDatabaseContainerEnvironmentSettings, string environment) : IDatabaseService
    {
        public string Environment { get; } = environment;

        private async Task<OperationResult> CreateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            try
            {
                var dtoOperationResult = new DTO.OperationResult();

                var userExists = await CheckUserExistsAsync(username, cancellationToken);

                if (userExists)
                {
                    var alterUserSql = $"ALTER USER \\\"{username}\\\" WITH SUPERUSER CREATEDB CREATEROLE LOGIN PASSWORD '{EscapeSqlString(password)}'";

                    dtoOperationResult = await ExecutePsqlCommandAsync(alterUserSql, cancellationToken);
                    if (!dtoOperationResult.Success)
                        return OperationResult.Fail($"Falha ao atualizar usuário: {dtoOperationResult.Message}");

                    return OperationResult.Ok($"Usuário atualizado ({Environment})", $"Usuário: {username} - Permissões: SUPERUSER, CREATEDB, CREATEROLE, LOGIN");
                }

                var createUserSql = $"CREATE USER \\\"{username}\\\" WITH SUPERUSER CREATEDB CREATEROLE LOGIN PASSWORD '{EscapeSqlString(password)}'";

                dtoOperationResult = await ExecutePsqlCommandAsync(createUserSql, cancellationToken);
                if (!dtoOperationResult.Success)
                    return OperationResult.Fail($"Falha ao criar usuário: {dtoOperationResult.Message}");

                return OperationResult.Ok($"Usuário criado ({Environment})", $"Usuário: {username} - Permissões: SUPERUSER, CREATEDB, CREATEROLE, LOGIN");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao configurar usuário ({Environment})", ex);
            }
        }

        private async Task<bool> CheckUserExistsAsync(string username, CancellationToken cancellationToken)
        {
            var sql = $"SELECT 1 FROM pg_roles WHERE rolname = '{EscapeSqlString(username)}'";
            var command = BuildDockerExecCommand(sql);
            var dtoOperationResult = await sshService.ExecuteCommandAsync(command, cancellationToken);
            return dtoOperationResult.Success && dtoOperationResult.Details?.Trim() == "1";
        }

        private async Task<OperationResult> ExecutePsqlCommandAsync(string sql, CancellationToken cancellationToken)
        {
            var command = BuildDockerExecCommand(sql);
            return await sshService.ExecuteCommandAsync(command, cancellationToken);
        }

        private string BuildDockerExecCommand(string sql, string? database = null)
        {
            var dbArg = database != null ? $"-d \"{database}\"" : string.Empty;
            return $"docker exec {dtoDatabaseContainerEnvironmentSettings.ContainerName} psql -U {dtoDatabaseContainerEnvironmentSettings.AdminUser} {dbArg} -t -c \"{sql}\"";
        }

        private static string EscapeSqlString(string value)
        {
            return value.Replace("'", "''").Replace("\"", "\\\"");
        }

        public async Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var command = BuildDockerExecCommand("SELECT version()");
                var dtoOperationResult = await sshService.ExecuteCommandAsync(command, cancellationToken);

                if (dtoOperationResult.Success)
                    return OperationResult.Ok($"Conectado ao PostgreSQL via container ({Environment})", $"Container: {dtoDatabaseContainerEnvironmentSettings.ContainerName}");

                return OperationResult.Fail($"Falha ao conectar ao PostgreSQL ({Environment}): {dtoOperationResult.Message}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao conectar ao PostgreSQL ({Environment})", ex);
            }
        }

        public async Task<OperationResult> CreateUserAndDatabaseAsync(string databaseName, string username, string password, CancellationToken cancellationToken = default)
        {
            return await CreateUserAsync(username, password, cancellationToken);
        }
    }
}