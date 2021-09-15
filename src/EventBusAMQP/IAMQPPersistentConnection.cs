using System;
using RabbitMQ.Client;

namespace OPMMetamodel.BuildingBlocks.EventBusAMQP
{
    public interface IAMQPPersistentConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}