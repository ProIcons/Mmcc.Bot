﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.MemberApplications
{
    /// <summary>
    /// Gets recent applications by status.
    /// </summary>
    public class GetByStatus
    {
        /// <summary>
        /// Query to get recent applications by status.
        /// </summary>
        public class Query : IRequest<Result<IList<MemberApplication>>>
        {
            /// <summary>
            /// Application status.
            /// </summary>
            public ApplicationStatus ApplicationStatus { get; set; }
        
            /// <summary>
            /// Limit of how many applications to get.
            /// </summary>
            /// <remarks>If set to <code>null</code> 20 applications will be received.</remarks>
            public int? Limit { get; set; }
            
            /// <summary>
            /// Whether to sort the applications by ID in descending order.
            /// </summary>
            public bool SortByDescending { get; set; }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<IList<MemberApplication>>>
        {
            private readonly BotContext _context;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The db context.</param>
            public Handler(BotContext context)
            {
                _context = context;
            }

            /// <inheritdoc />
            public async Task<Result<IList<MemberApplication>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var res = _context.MemberApplications
                        .AsNoTracking()
                        .Where(app => app.AppStatus == request.ApplicationStatus);
                    res = (request.SortByDescending
                            ? res.OrderByDescending(app => app.MemberApplicationId)
                            : res.OrderBy(app => app.MemberApplicationId))
                        .Take(request.Limit ?? 20);
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