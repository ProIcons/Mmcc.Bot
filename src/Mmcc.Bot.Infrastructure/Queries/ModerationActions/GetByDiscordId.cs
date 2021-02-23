﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.ModerationActions
{
    /// <summary>
    /// Gets moderation actions for a given Discord user.
    /// </summary>
    public class GetByDiscordId
    {
        /// <summary>
        /// Query to get moderation actions by Discord user ID.
        /// </summary>
        public class Query : IRequest<Result<IList<ModerationAction>>>
        {
            /// <summary>
            /// ID of the guild.
            /// </summary>
            public Snowflake GuildId { get; set; }
            
            /// <summary>
            /// ID of the Discord user.
            /// </summary>
            public ulong DiscordUserId { get; set; }
        }

        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<IList<ModerationAction>>>
        {
            private readonly BotContext _context;

            public Handler(BotContext context)
            {
                _context = context;
            }
            
            /// <inheritdoc />
            public async Task<Result<IList<ModerationAction>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var res = _context.ModerationActions
                        .AsNoTracking()
                        .Where(ma => ma.UserDiscordId != null && ma.UserDiscordId == request.DiscordUserId);
                    return await res.ToListAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}