﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;

namespace Mmcc.Bot.Infrastructure.HostedServices
{
    public class BroadcastsHostedService : IHostedService, IDisposable
    {
        private const int IntervalInMinutes = 7;
        
        private readonly ILogger<BroadcastsHostedService> _logger;
        private readonly IPolychatService _ps;
        
        private readonly string? _id;
        private readonly string? _prefix;
        private readonly List<string>? _broadcastMessages;

        private Timer? _timer;
        private int _broadcastMessagesIndex;

        public BroadcastsHostedService(ILogger<BroadcastsHostedService> logger, IPolychatService ps, PolychatSettings polychatSettings)
        {
            _logger = logger;
            _ps = ps;
            
            _id = polychatSettings.BroadcastsSettings?.Id;
            _prefix = polychatSettings.BroadcastsSettings?.Prefix;
            _broadcastMessages = polychatSettings.BroadcastsSettings?.BroadcastMessages;
            
            _broadcastMessagesIndex = 0;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {service}...", nameof(BroadcastsHostedService));

            if (_broadcastMessages is null
                || _broadcastMessages.Count == 0
                || _id is null
                || _prefix is null
            )
            {
                _logger.LogWarning("Broadcasts configuration is invalid or not set. Stopping the service...");
                _logger.LogInformation("Stopped {service}...", nameof(BroadcastsHostedService));
                return Task.CompletedTask;
            }

            _logger.LogInformation("Started {service}...", nameof(BroadcastsHostedService));

            _timer = new Timer(RunIteration, null, TimeSpan.Zero, TimeSpan.FromMinutes(IntervalInMinutes));
            
            return Task.CompletedTask;
        }

        private void RunIteration(object? state)
        {
            try
            {
                Broadcast();
            }
            catch (Exception e)
            {
                _logger.LogError("Broadcast error.", e);
            }
            
            _broadcastMessagesIndex = (_broadcastMessagesIndex + 1) % _broadcastMessages!.Count;
        }

        private void Broadcast()
        {
            var msg = _broadcastMessages![_broadcastMessagesIndex]!;
            var proto = new ChatMessage
            {
                ServerId = _id!,
                Message = $"{_prefix} {msg}",
                MessageOffset = _prefix!.Length - 1
            };
            _ps.BroadcastMessage(proto);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping {service}...", nameof(BroadcastsHostedService));

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}