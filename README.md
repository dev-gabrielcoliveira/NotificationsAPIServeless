# NotificationsAPI.Serverless

> Microsserviço de notificações serverless desenvolvido com Azure Functions (.NET 8) e Azure Storage Queues para o Tech Challenge (Fase 3).

---

## 🛠️ Tecnologias Utilizadas

* **.NET 8** (Isolated Worker Model)
* **Azure Functions** (v4)
* **Azure Storage Queues** (para mensageria assíncrona desacoplada)
* **Azurite** (Emulador local de armazenamento do Azure)

---

## 🏗️ Arquitetura e Decisão de Design

Este projeto faz parte de um ecossistema de microsserviços. Para atender aos requisitos de **arquitetura orientada a eventos**, **eficiência de recursos** e **desacoplamento** sem a necessidade de um broker containerizado pesado (como o RabbitMQ), a aplicação foi migrada para um modelo **Serverless**.

* **Produtor (`UsersAPI` / Outros serviços):** Publica mensagens diretamente na Fila do Azure Storage (`notifications-queue`) assim que eventos de negócios ocorrem (ex: cadastro de usuário).
* **Consumidor (`NotificationsAPI.Serverless`):** Uma Azure Function acionada por gatilho de fila (`[QueueTrigger]`). Ela executa sob demanda, processa a mensagem e envia a notificação de forma assídua e sem manter recursos ociosos.

---

## 🚀 Como Executar o Projeto Localmente

### Pré-requisitos
Certifique-se de ter instalado em sua máquina:
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
* [Node.js e Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (para emular a fila localmente)

### Passo a Passo

1. **Inicie o emulador Azurite** (para simular a Fila do Azure localmente):
   ```powershell
   azurite --silent

**Configure o arquivo `local.settings.json` na raiz do projeto com a connection string de desenvolvimento:**

```powershell
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
} 

**Execute a Azure Function:**

```powershell dotnet run