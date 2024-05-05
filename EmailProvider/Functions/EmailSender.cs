using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Models;
using EmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProvider.Functions
{
    public class EmailSender(ILogger<EmailSender> logger, IEmailSenderService emailSenderService)
    {
        private readonly ILogger<EmailSender> _logger = logger;
        private readonly IEmailSenderService _emailSenderService = emailSenderService;

        [Function(nameof(EmailSender))]
        public async Task Run([ServiceBusTrigger("email_request", Connection = "ServiceBus")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
        {
            try
            {
                var request = _emailSenderService.UnpackEmailRequest(message);
                if (request != null && !string.IsNullOrEmpty(request.To))
                {
                    if (_emailSenderService.SendEmail(request))
                    {
                        await messageActions.CompleteMessageAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error :: EmailSender.Run :: {ex.Message} ");
            }
        }


    }
}
