using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    [Group("moderation", "mod", "m")]
    [RequireGuild]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public class ModerationCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;

        /// <summary>
        /// Instantiates a new instance of <see cref="ModerationCommands"/> class. 
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public ModerationCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            ColourPalette colourPalette
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
        }

        [Command("deactivate", "disable")]
        [Description("Deactivates a moderation action.")]
        public async Task<IResult> Deactivate(int id)
        {
            var queryResult = await _mediator.Send(new TestQuery.Query(null));
            if (queryResult.IsSuccess)
            {
                Console.WriteLine("success");
            }
            else
            {
                Console.WriteLine("fail");
            }

            return Result.FromSuccess();
        }
    }
}