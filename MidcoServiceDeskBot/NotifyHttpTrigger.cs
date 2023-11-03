using MidcoServiceDeskBot.Models;
using AdaptiveCards.Templating;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.TeamsFx.Conversation;
using Newtonsoft.Json;

using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using System.Collections.Generic;

namespace MidcoServiceDeskBot
{
    public sealed class NotifyHttpTrigger
    {
        private readonly ConversationBot _conversation;
        private readonly ILogger<NotifyHttpTrigger> _log;

        public NotifyHttpTrigger(ConversationBot conversation, ILogger<NotifyHttpTrigger> log)
        {
            _conversation = conversation;
            _log = log;
        }

        [FunctionName("NotifyHttpTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/notification")] HttpRequest req, ExecutionContext context)
        {
            _log.LogInformation("NotifyHttpTrigger is triggered.");
            
            // Read adaptive card template
            var adaptiveCardFilePath = Path.Combine(context.FunctionAppDirectory, "Resources", "PasswordExpirationCard.json");
            var cardTemplate = await File.ReadAllTextAsync(adaptiveCardFilePath, req.HttpContext.RequestAborted);

            // Read body and build card model.
            string messageContent = await req.ReadAsStringAsync();
            var passwordExpirationModel = JsonConvert.DeserializeObject<List<PasswordExpirationModel>>(messageContent);

            var pageSize = 100;
            string continuationToken = null;
            do
            {
                //_conversation.Notification.FindAllMembersAsync()
                var pagedInstallations = await _conversation.Notification.GetPagedInstallationsAsync(pageSize, continuationToken, req.HttpContext.RequestAborted);
                continuationToken = pagedInstallations.ContinuationToken;
                var installations = pagedInstallations.Data;
                foreach (var installation in installations)
                {
                    // Build and send adaptive card
                    var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand(passwordExpirationModel);
                    await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), req.HttpContext.RequestAborted);
                }

            } while (!string.IsNullOrEmpty(continuationToken));

            return new OkResult();
        }
    }
}
