using System;
using Autofac;
using Microsoft.Extensions.Logging;
using OPMMetamodel.BuildingBlocks.EventBus;
using OPMMetamodel.BuildingBlocks.EventBus.Abstractions;
using OPMMetamodel.BuildingBlocks.EventBus.Events;
using RabbitMQ.Client;

namespace OPMMetamodel.BuildingBlocks.EventBusAMQP
{
    public class EventBusAMQP :  IEventBus, IDisposable
    {
        
        const string BROKER_NAME = "event_bus";
        const string AUTOFAC_SCOPE_NAME = "event_bus";

        private readonly IAMQPPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusAMQP> _logger;
        private readonly IEventBusSubscriptionManager _subscriptionManager;
        private readonly ILifetimeScope _autofac;
        private readonly int _retryCount;

        private IModel _consumerChannel;
        private string _queueName;
        
        /// <inheritdoc />
        public void Publish(IntegrationEvent @event)
        {
        }

        /// <inheritdoc />
        public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
        }

        /// <inheritdoc />
        public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subscriptionManager.GetEventKey<T>();
            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subscriptionManager.RemoveSubscription<T, TH>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subscriptionManager.Clear();
        }
        
        
    }
}