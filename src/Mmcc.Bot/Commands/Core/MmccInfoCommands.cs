﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Common.Extensions.Caching;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.RemoraAbstractions;
using Mmcc.Bot.RemoraAbstractions.Services;
using Mmcc.Bot.RemoraAbstractions.Ui;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Core
{
    public class MmccInfoCommands : CommandGroup
    {
        private readonly ICommandResponder _responder;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestWebhookAPI _webhookApi;
        private readonly IButtonHandlerRepository _handlerRepository;
        private readonly IInteractionResponder _interactionResponder;

        public MmccInfoCommands(ICommandResponder responder, IDiscordRestInteractionAPI interactionApi, IDiscordRestChannelAPI channelApi, IButtonHandlerRepository handlerRepository, IDiscordRestWebhookAPI webhookApi, IInteractionResponder interactionResponder)
        {
            _responder = responder;
            _interactionApi = interactionApi;
            _channelApi = channelApi;
            _handlerRepository = handlerRepository;
            _webhookApi = webhookApi;
            _interactionResponder = interactionResponder;
        }

        [Command("mmcc")]
        [Description("Shows useful MMCC links")]
        public async Task<IResult> Mmcc()
        {
            var usefulLinks = new List<IMessageComponent>
            {
                new ActionRowComponent(new List<ButtonComponent>
                {
                    new(ButtonComponentStyle.Link, "Website", new PartialEmoji(new Snowflake(863798570602856469)), URL: MmccUrls.Website),
                    new(ButtonComponentStyle.Link, "Donate", new PartialEmoji(Name: "❤️"), URL: MmccUrls.Donations),
                    new(ButtonComponentStyle.Link, "Wiki", new PartialEmoji(Name: "📖"), URL: MmccUrls.Wiki),
                    new(ButtonComponentStyle.Link, "Forum", new PartialEmoji(Name: "🗣️"), URL: MmccUrls.Forum),
                    new(ButtonComponentStyle.Link, "GitHub", new PartialEmoji(new Snowflake(453413238638641163)), URL: MmccUrls.GitHub)
                })
            };

            return await _responder.RespondWithComponents(usefulLinks, "Useful links");
        }

        // TODO: remove once app buttons are implemented;
        [Command("test")]
        public async Task<IResult> Test(string tester)
        {
            var testButton = new ButtonBuilder(ButtonComponentStyle.Primary)
                .WithLabel("Test")
                .WithHandler(async ev => await _interactionResponder.SendFollowup(ev.Token, new Embed(tester)))
                .WithRequiredPermission(DiscordPermission.BanMembers)
                .Build()
                .RegisterWith(_handlerRepository);
            return await _responder.RespondWithComponents(ActionRowUtils.FromButtons(testButton), "Test buttons");
        }
    }
}