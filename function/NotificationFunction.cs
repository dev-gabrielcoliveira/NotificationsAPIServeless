using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace NotificationsAPI.Serverless;

public class NotificationFunction
{
    private readonly ILogger<NotificationFunction> _logger;

    public NotificationFunction(ILogger<NotificationFunction> logger)
    {
        _logger = logger;
    }

    // 1. Produtor: Recebe a requisição HTTP POST, converte para Base64 e envia para a Fila
    [Function("CreateNotification")]
    public async Task<MultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications")] HttpRequestData req)
    {
        _logger.LogInformation("Recebida requisição HTTP para criar notificação.");

        using var reader = new StreamReader(req.Body);
        var requestBody = await reader.ReadToEndAsync();
        
        var notification = JsonSerializer.Deserialize<NotificationDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        string jsonMessage = JsonSerializer.Serialize(notification);
        string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonMessage));

        var response = req.CreateResponse(System.Net.HttpStatusCode.Accepted);
        await response.WriteStringAsync("Notificação aceita e enfileirada para processamento com sucesso!");

        return new MultiResponse
        {
            Message = base64Message,
            HttpResponse = response
        };
    }

    // 2. Consumidor: Acionado automaticamente quando chega nova mensagem na fila
    [Function("ProcessNotificationQueue")]
    public void ProcessQueue(
        [QueueTrigger("notifications-queue", Connection = "AzureWebJobsStorage")] string base64Message)
    {
        var bytes = Convert.FromBase64String(base64Message);
        var jsonMessage = Encoding.UTF8.GetString(bytes);
        var notification = JsonSerializer.Deserialize<NotificationDto>(jsonMessage);

        _logger.LogInformation($"[PROCESSAMENTO DE FILA] Destinatário: {notification?.Recipient} | Assunto: {notification?.Subject} | Corpo: {notification?.Body}");
    }
}

public class MultiResponse
{
    [QueueOutput("notifications-queue", Connection = "AzureWebJobsStorage")]
    public string Message { get; set; } = string.Empty;
    public HttpResponseData HttpResponse { get; set; } = null!;
}