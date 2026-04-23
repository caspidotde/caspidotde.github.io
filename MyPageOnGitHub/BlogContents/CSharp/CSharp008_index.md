<!-- Send MQTT to Blazor through SignalR -->

##### Send MQTT to Blazor through SignalR


````csharp

// Program.cs

builder.Services.AddSignalR();
builder.Services.AddHostedService<MQTTBackgroundService>();
builder.Services.AddCors();
...
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.MapHub<NotificationHub>("/notificationHub");

````


````csharp

// NotificationHub.cs

public class NotificationHub: Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).ReceiveNotification("Connection", 
            $"You are connected to the notification hub: {Context.User?.Identity?.Name}");
        await base.OnConnectedAsync();
    }
}

public interface INotificationClient
{
    Task ReceiveNotification(string topic, string message);
}

public class MQTTMessage
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Topic { get; set; }
    public string? Message { get; set; }
}


````

````csharp

// MQTTBackgroundService.cs

public class MQTTBackgroundService: IHostedService
{
    private readonly ILogger _logger;
    private readonly MqttClientFactory mqttFactory;
    private readonly IMqttClient mqttClient;
    private readonly MqttClientOptions mqttClientOptions;
    private readonly IHubContext<NotificationHub, INotificationClient> _context;


    public MQTTBackgroundService(
        ILogger<MQTTBackgroundService> logger,
        IHubContext<NotificationHub, INotificationClient> notificationHubContext
        )
    {
        _logger = logger;
        _context = notificationHubContext;

        mqttFactory = new MqttClientFactory();
        mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += MqttApplicationMessageReceivedAsync;

        mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer({{mqttServerIP}})
            .Build();        
    }

    private async Task MqttApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
        string topic = args.ApplicationMessage.Topic;
        _logger.LogTrace($"Topic: {topic} - {payload}");
        await _context.Clients.All.ReceiveNotification(topic, payload);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Background Service is started.");
        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => { f.WithTopic("myTopic"); })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Background Service is stopping.");
        
        var mqttSubscribeOptions = mqttFactory.CreateUnsubscribeOptionsBuilder().WithTopicFilter("myTopic").Build();
        await mqttClient.UnsubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, cancellationToken);

        mqttClient.Dispose();
    }


````

````csharp

// TestMQTT.razor

@implements IAsyncDisposable
@inject ILoggerFactory loggerFactory
@inject IConfiguration configuration

<MudText Typo="Typo.h6">Test</MudText>

<MudTable T="MQTTMessage" Items="messages" >
    <HeaderContent>
        <MudTh>Topic</MudTh>        
        <MudTh>Message</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Timestamp">@context.Timestamp.ToShortDateString() @context.Timestamp.ToLongTimeString()</MudTd>
        <MudTd DataLabel="Topic">@context.Topic</MudTd>
        <MudTd DataLabel="Message">@context.Message</MudTd>
    </RowTemplate>
</MudTable>

@code {

    private ILogger logger = default!;
    private List<MQTTMessage> messages = default!;
    private HubConnection? _hubConnection;

    protected override async Task OnInitializedAsync()
    {
        logger = loggerFactory.CreateLogger<TestPage>();

        messages = new List<MQTTMessage>() { new MQTTMessage() {Topic = "TestPage", Message = "Test message"}};

        var applicationUrl = configuration.GetValue<string>("YourApp:ApplicationUrl") ?? "";
        var hubUrl = $"{applicationUrl}notificationHub";        
        // logger.LogInformation(hubUrl);

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                options.UseDefaultCredentials = true;
            })
            .WithAutomaticReconnect()
            .Build();

        // logger.LogInformation(_hubConnection.ConnectionId);

        _hubConnection.On<string, string>("ReceiveNotification", (topic, message) =>
        {
            var mqttMessage = new MQTTMessage() { Topic = topic, Message = message, Timestamp = DateTime.Now };
            messages.Insert(0, mqttMessage);
            InvokeAsync(StateHasChanged);
        });

        CancellationToken token = new CancellationTokenSource().Token;
        await _hubConnection.StartAsync(token);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}


````