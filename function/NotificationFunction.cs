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

        // Apenas serializa para JSON limpo. O Azure Storage Queue já cuida do Base64 sozinho!
        string jsonMessage = JsonSerializer.Serialize(notification);

        var response = req.CreateResponse(System.Net.HttpStatusCode.Accepted);
        await response.WriteStringAsync("Notificação aceita e enfileirada para processamento com sucesso!");

        return new MultiResponse
        {
            Message = jsonMessage,
            HttpResponse = response
        };
    }

    [Function("ProcessNotificationQueue")]
    public void ProcessQueue(
        [QueueTrigger("notifications-v3", Connection = "AzureWebJobsStorage")] string message)
    {
        var partes = message.Split('|');

        string recipient = partes[0];
        string subject = partes[1];
        string body = partes[2];

        _logger.LogInformation($"Destinatário: {recipient} | Assunto: {subject} | Corpo: {body}");
    }
}

public class MultiResponse
{
    [QueueOutput("notifications-v2", Connection = "AzureWebJobsStorage")]
    public string Message { get; set; } = string.Empty;
    public HttpResponseData HttpResponse { get; set; } = null!;
}