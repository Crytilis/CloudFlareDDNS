using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using ZoneUpdater.Clients;

namespace ZoneUpdater.Services
{
    public class DDnsUpdaterService : BackgroundService
    {
        private readonly ILogger<DDnsUpdaterService> _logger;
        private readonly IConfiguration _configuration;
        private readonly CloudFlareClient _client;
        private int? _udpPort;
        private readonly string? _zoneName;
        private readonly List<string>? _recordNames;

        public DDnsUpdaterService(ILogger<DDnsUpdaterService> logger, IConfiguration configuration, CloudFlareClient client)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _zoneName = _configuration["CloudFlare:ZoneName"];
            _recordNames = _configuration.GetSection("CloudFlare:RecordNames").Get<List<string>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _udpPort = Convert.ToInt32(_configuration.GetValue<string>("SyslogServer:UdpPort"));

            if (!_udpPort.HasValue)
            {
                _logger.LogError("UDP port is not configured.");
                return;
            }

            using var udpClient = new UdpClient(_udpPort.Value);

            _logger.LogInformation($"Syslog: Listening for messages on UDP port {_udpPort}...");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ReceiveMessageAsync(udpClient, stoppingToken);
            }
        }

        private async Task ReceiveMessageAsync(UdpClient udpClient, CancellationToken stoppingToken)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var logMessage = Encoding.UTF8.GetString(result.Buffer);
                await ProcessLogMessageAsync(logMessage);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while receiving UDP message.");
            }
        }

        private async Task ProcessLogMessageAsync(string logMessage)
        {
            var regex = new Regex(@"dhcp client: bound (\d+\.\d+\.\d+\.\d+)");
            var match = regex.Match(logMessage);

            if (!match.Success) return;
            var newIp = match.Groups[1].Value;
            _logger.LogInformation($"Syslog: New WAN IP detected: {newIp}");

            await UpdateDnsRecordsAsync(newIp);
        }

        private async Task UpdateDnsRecordsAsync(string newIp)
        {
            if (string.IsNullOrWhiteSpace(_zoneName) || _recordNames == null)
            {
                _logger.LogError("DDNS: CloudFlare settings are not configured properly.");
                return;
            }

            var zoneId = await _client.GetZoneIdAsync(_zoneName);
            if (zoneId == null)
            {
                _logger.LogError($"DDNS: {_zoneName} does not exist within your CloudFlare account or the API key provided does not have access.");
                return;
            }

            var allRecordsInZone = await _client.GetRecordIdsAsync(zoneId, _recordNames);
            if (allRecordsInZone == null)
            {
                _logger.LogError($"DDNS: {_zoneName} does not contain any DNS records or the API key provided does not have access.");
                return;
            }

            foreach (var record in allRecordsInZone)
            {
                var response = await _client.CreateOrUpdateRecordAsync(zoneId, record.Value, record.Key, newIp);
                if (response != null)
                {
                    _logger.LogInformation($"DDNS: {response.Result?.Name} was updated to {response.Result?.Content} with a TTL of {response.Result?.Ttl}");
                }
            }
        }
    }
}