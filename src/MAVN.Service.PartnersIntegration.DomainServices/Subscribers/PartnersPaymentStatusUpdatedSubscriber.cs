using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PartnersIntegration.Domain.Services;
using Lykke.Service.PartnersPayments.Contract;

namespace MAVN.Service.PartnersIntegration.DomainServices.Subscribers
{
    public class PartnersPaymentStatusUpdatedSubscriber : JsonRabbitSubscriber<PartnersPaymentStatusUpdatedEvent>
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILog _log;

        public PartnersPaymentStatusUpdatedSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            IPaymentsService paymentsService,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _paymentsService = paymentsService;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(PartnersPaymentStatusUpdatedEvent message)
        {
            _log.Info($"Started processing partners payment status updated event for PaymentRequestId: {message.PaymentRequestId}",
                message);

            try
            {
                await _paymentsService.ProcessPartnersPaymentStatusUpdatedEvent(message);
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to process partners payment status updated event", message);
                throw;
            }

            _log.Info($"Finished processing partners payment status updated event for PaymentRequestId: {message.PaymentRequestId}",
                message);
        }
    }
}
