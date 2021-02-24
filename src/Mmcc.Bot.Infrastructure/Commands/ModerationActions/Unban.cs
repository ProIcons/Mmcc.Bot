﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.ModerationActions
{
    /// <summary>
    /// Unbans a user.
    /// </summary>
    public class Unban
    {
        /// <summary>
        /// Command to unban a user.
        /// </summary>
        public class Command : IRequest<Result>
        {
            /// <summary>
            /// Moderation action.
            /// </summary>
            public ModerationAction ModerationAction { get; set; } = null!;
            
            /// <summary>
            /// ID of the channel to which polychat2 will send the confirmation message.
            /// </summary>
            public Snowflake ChannelId { get; set; }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly BotContext _context;
            private readonly IPolychatCommunicationService _pcs;
            private readonly IDiscordRestGuildAPI _guildApi;
            private readonly IDiscordRestUserAPI _userApi;
            private readonly IDiscordRestChannelAPI _channelApi;
            private readonly ColourPalette _colourPalette;
            private readonly ILogger<Handler> _logger;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/> class.
            /// </summary>
            /// <param name="context">The DB context.</param>
            /// <param name="pcs">The polychat communication service.</param>
            /// <param name="guildApi">The guild API.</param>
            /// <param name="userApi">The user API.</param>
            /// <param name="channelApi">The channel API.</param>
            /// <param name="colourPalette">The colour palette.</param>
            /// <param name="logger">The logger.</param>
            public Handler(
                BotContext context,
                IPolychatCommunicationService pcs,
                IDiscordRestGuildAPI guildApi,
                IDiscordRestUserAPI userApi,
                IDiscordRestChannelAPI channelApi,
                ColourPalette colourPalette,
                ILogger<Handler> logger
            )
            {
                _context = context;
                _pcs = pcs;
                _guildApi = guildApi;
                _userApi = userApi;
                _channelApi = channelApi;
                _colourPalette = colourPalette;
                _logger = logger;
            }

            /// <inheritdoc />
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var ma = request.ModerationAction;
                if (ma.ModerationActionType != ModerationActionType.Ban)
                    return new GenericError(
                        $"Wrong moderation action type. Expected: {ModerationActionType.Ban}, got: {ma.ModerationActionType}"); 
                if (!ma.IsActive) return new GenericError("Moderation action was already inactive.");

                if (ma.UserIgn is not null)
                {
                    var protobufMessage = new GenericServerCommand
                    {
                        ServerId = "<all>",
                        Command = new GenericCommand
                        {
                            DefaultCommand = "ban",
                            DiscordCommandName = "ban",
                            DiscordChannelId = request.ChannelId.Value.ToString(),
                            Args = {request.ModerationAction.UserIgn}
                        }
                    };
                    var sendProtobufMessageResult = await _pcs.SendProtobufMessage(protobufMessage);
                    if (!sendProtobufMessageResult.IsSuccess)
                    {
                        return new PolychatError(
                            "Could not communicate with polychat2's central server. Please see the logs.");
                    }
                }
                
                if (request.ModerationAction.UserDiscordId is not null)
                {
                    var userDiscordIdSnowflake = new Snowflake(request.ModerationAction.UserDiscordId.Value);
                    var banResult = await _guildApi.RemoveGuildBanAsync(
                        new(request.ModerationAction.GuildId),
                        new(request.ModerationAction.UserDiscordId.Value),
                        cancellationToken
                    );

                    if (!banResult.IsSuccess)
                    {
                        return Result.FromError(banResult);
                    }

                    var embed = new Embed
                    {
                        Title = "You have been unbanned from Modded Minecraft Club.",
                        Colour = _colourPalette.Green,
                        Thumbnail = EmbedProperties.MmccLogoThumbnail
                    };

                    var createDmResult = await _userApi.CreateDMAsync(userDiscordIdSnowflake, cancellationToken);
                    const string warningMsg =
                        "Failed to send a DM notification to the user. It may be because they have blocked the bot or don't share any servers. This warning can in most cases be ignored.";
                    if (!createDmResult.IsSuccess || createDmResult.Entity is null)
                    {
                        _logger.LogWarning(warningMsg);
                    }
                    else
                    {
                        var sendDmResult = await _channelApi.CreateMessageAsync(createDmResult.Entity.ID, embed: embed,
                            ct: cancellationToken);
                        if (!sendDmResult.IsSuccess)
                        {
                            _logger.LogWarning(warningMsg);
                        }
                    }
                }

                try
                {
                    ma.IsActive = false;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
                
                return Result.FromSuccess();
            }
        }
    }
}