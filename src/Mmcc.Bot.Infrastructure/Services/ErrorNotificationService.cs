﻿using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Services
{
    /// <summary>
    /// Service that handles notifying the user that the command has failed.
    /// </summary>
    public class ErrorNotificationService : IExecutionEventService
    {
        private readonly ILogger<ErrorNotificationService> _logger;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ColourPalette _colourPalette;

        /// <summary>
        /// Instantiates a new instance of <see cref="ErrorNotificationService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public ErrorNotificationService(
            ILogger<ErrorNotificationService> logger,
            IDiscordRestChannelAPI channelApi,
            ColourPalette colourPalette
        )
        {
            _logger = logger;
            _channelApi = channelApi;
            _colourPalette = colourPalette;
        }

        /// <inheritdoc />
        public Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct)
        {
            return Task.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public async Task<Result> AfterExecutionAsync(
            ICommandContext context,
            IResult executionResult,
            CancellationToken ct
        )
        {
            if (executionResult.IsSuccess)
            {
                return Result.FromSuccess();
            }

            var err = executionResult.Inner is null
                      || executionResult.Inner.IsSuccess
                      || executionResult.Inner.Error is null
                ? executionResult.Error
                : executionResult.Inner.Error;

            var errorEmbed = new Embed(Thumbnail: EmbedProperties.MmccLogoThumbnail, Colour: _colourPalette.Red);
            errorEmbed = err switch
            {
                ValidationError => errorEmbed with
                {
                    Title = ":exclamation: Validation error.",
                    Description = err.Message
                },
                NotFoundError => errorEmbed with
                {
                    Title = ":x: Resource not found.",
                    Description = err.Message
                },
                null => errorEmbed with
                {
                    Title = ":exclamation: Error.",
                    Description = "Unknown error."
                },
                _ => errorEmbed with
                {
                    Title = $":x: {err.GetType()} error.",
                    Description = err.Message
                }
            };

            var sendEmbedResult = await _channelApi.CreateMessageAsync(context.ChannelID, embed: errorEmbed, ct: ct);
            return !sendEmbedResult.IsSuccess
                ? Result.FromError(sendEmbedResult)
                : Result.FromSuccess();
        }
    }
}