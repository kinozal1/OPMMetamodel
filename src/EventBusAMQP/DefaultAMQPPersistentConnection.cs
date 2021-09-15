using System;
using System.IO;
using System.Net.Sockets;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace OPMMetamodel.BuildingBlocks.EventBusAMQP
{
    public class DefaultAMQPPersistentConnection : IAMQPPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private readonly ILogger<DefaultAMQPPersistentConnection> _logger;
        IConnection _connection;
        bool _disposed;
        
        object sync_root = new object();

        public DefaultAMQPPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultAMQPPersistentConnection> logger, int retryCount =5)
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
            _logger = logger;
        }
        

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        /// <inheritdoc />
        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        /// <inheritdoc />
        public bool TryConnect()
        {
            _logger.LogInformation("AMQP Client is trying to connect");
            lock (sync_root)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                        {
                            _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                        }
                    );
                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                        .CreateConnection();
                });
                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation("AMQP Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName);

                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: AMQP connections could not be created and opened");

                    return false;
                }
            }
        }

        /// <inheritdoc />
        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No AMQP connections are available to perform this action");
            }

            return _connection.CreateModel();
        }
        
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A AMQP connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A AMQP connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.LogWarning("A AMQP connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}