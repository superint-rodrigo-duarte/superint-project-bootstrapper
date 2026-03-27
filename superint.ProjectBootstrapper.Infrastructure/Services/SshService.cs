using Renci.SshNet;
using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;

namespace superint.ProjectBootstrapper.Infrastructure.Services
{
    public sealed class SshService(ServerSettings dtoServerSettings, string environment) : ISshService, IDisposable
    {
        private SshClient? _sshClient;
        private SftpClient? _sftpClient;

        public string Environment { get; } = environment;

        private ConnectionInfo CreateConnectionInfo()
        {
            var authenticationMethod = new List<AuthenticationMethod>();

            if (!string.IsNullOrEmpty(dtoServerSettings.PrivateKeyPath) && File.Exists(dtoServerSettings.PrivateKeyPath))
            {
                var privateKeyFile = new PrivateKeyFile(dtoServerSettings.PrivateKeyPath);
                authenticationMethod.Add(new PrivateKeyAuthenticationMethod(dtoServerSettings.Username, privateKeyFile));
            }

            if (!string.IsNullOrEmpty(dtoServerSettings.Password))
            {
                authenticationMethod.Add(new PasswordAuthenticationMethod(dtoServerSettings.Username, dtoServerSettings.Password));
            }

            if (authenticationMethod.Count == 0)
            {
                throw new InvalidOperationException($"Nenhum método de autenticação configurado para o servidor {Environment}");
            }

            return new ConnectionInfo(dtoServerSettings.Host, dtoServerSettings.Port, dtoServerSettings.Username, [.. authenticationMethod]);
        }

        private SshClient GetSshClient()
        {
            if (_sshClient == null || !_sshClient.IsConnected)
            {
                _sshClient?.Dispose();
                _sshClient = new SshClient(CreateConnectionInfo());
                _sshClient.Connect();
            }
            return _sshClient;
        }

        private SftpClient GetSftpClient()
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
            {
                _sftpClient?.Dispose();
                _sftpClient = new SftpClient(CreateConnectionInfo());
                _sftpClient.Connect();
            }
            return _sftpClient;
        }

        public Task<OperationResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sshClient = GetSshClient();

                var sshCommand = sshClient.RunCommand("echo 'Connection successful'");
                if (sshCommand.ExitStatus == 0)
                    return Task.FromResult(OperationResult.Ok($"Conectado ao servidor ({Environment})", $"{dtoServerSettings.Host}:{dtoServerSettings.Port}"));

                return Task.FromResult(OperationResult.Fail($"Falha na conexão: {sshCommand.Error}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult.Fail($"Falha ao conectar ao servidor ({Environment})", ex));
            }
        }

        public Task<OperationResult> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sshClient = GetSshClient();

                var sshCommand = sshClient.RunCommand(command);
                if (sshCommand.ExitStatus == 0)
                    return Task.FromResult(OperationResult.Ok("Comando executado com sucesso", sshCommand.Result.Trim()));

                return Task.FromResult(OperationResult.Fail($"Comando falhou (exit {sshCommand.ExitStatus}): {sshCommand.Error}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult.Fail($"Falha ao executar comando no servidor ({Environment})", ex));
            }
        }

        public async Task<OperationResult> UploadFileAsync(string localContent, string remotePath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sftpClient = GetSftpClient();

                using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(localContent));
                await Task.Run(() => sftpClient.UploadFile(memoryStream, remotePath, true), cancellationToken);

                return OperationResult.Ok("Arquivo enviado com sucesso", remotePath);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Falha ao enviar arquivo para {remotePath}", ex);
            }
        }

        public Task<OperationResult> EnsureDirectoryExistsAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sshClient = GetSshClient();

                var sshCommand = sshClient.RunCommand($"mkdir -p \"{remotePath}\"");
                if (sshCommand.ExitStatus == 0)
                    return Task.FromResult(OperationResult.Ok("Diretório criado/verificado", remotePath));

                return Task.FromResult(OperationResult.Fail($"Falha ao criar diretório: {sshCommand.Error}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(OperationResult.Fail($"Falha ao criar diretório {remotePath}", ex));
            }
        }

        public Task<bool> DirectoryExistsAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sshClient = GetSshClient();
                var sshCommand = sshClient.RunCommand($"test -d \"{remotePath}\" && echo 'exists'");

                return Task.FromResult(sshCommand.Result.Trim() == "exists");
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public void Dispose()
        {
            _sshClient?.Dispose();
            _sftpClient?.Dispose();
        }
    }
}