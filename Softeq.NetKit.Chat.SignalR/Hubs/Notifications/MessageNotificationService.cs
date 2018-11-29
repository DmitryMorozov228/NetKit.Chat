﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.SignalR;
using Softeq.NetKit.Chat.Domain.Services.DomainServices;
using Softeq.NetKit.Chat.Domain.TransportModels.Response.Channel;
using Softeq.NetKit.Chat.Domain.TransportModels.Response.Message;

namespace Softeq.NetKit.Chat.SignalR.Hubs.Notifications
{
    public class MessageNotificationService : BaseNotificationService, IMessageNotificationService
    {
        private readonly IChannelService _channelService;

        public MessageNotificationService(
            IChannelMemberService channelMemberService,
            IMemberService memberService,
            IHubContext<ChatHub> hubContext,
            IChannelService channelService)
            : base(channelMemberService, memberService, hubContext)
        {
            Ensure.That(channelService).IsNotNull();

            _channelService = channelService;
        }

        public async Task OnAddMessage(MessageResponse message)
        {
            var clientIds = await GetNotMutedChannelClientConnectionIdsAsync(message.ChannelId);
            
            await HubContext.Clients.Clients(clientIds).SendAsync(HubEvents.MessageAdded, message);
        }

        public async Task OnDeleteMessage(ChannelSummaryResponse channelSummary, MessageResponse message)
        {
            var clientIds = await GetNotMutedChannelClientConnectionIdsAsync(message.ChannelId);
            
            await HubContext.Clients.Clients(clientIds).SendAsync(HubEvents.MessageDeleted, message.Id, channelSummary);
        }

        public async Task OnUpdateMessage(MessageResponse message)
        {
            var clientIds = await GetNotMutedChannelClientConnectionIdsAsync(message.ChannelId);
            
            await HubContext.Clients.Clients(clientIds).SendAsync(HubEvents.MessageUpdated, message);
        }

        public async Task OnAddMessageAttachment(Guid channelId)
        {
            var channel = await _channelService.GetChannelByIdAsync(channelId);

            var clientIds = await GetNotMutedChannelClientConnectionIdsAsync(channelId);
            
            await HubContext.Clients.Clients(clientIds).SendAsync(HubEvents.AttachmentAdded, channel.Name);
        }

        public async Task OnDeleteMessageAttachment(MessageResponse message)
        {
            var channel = await _channelService.GetChannelByIdAsync(message.ChannelId);

            var clientIds = await GetNotMutedChannelClientConnectionIdsAsync(channel.Id);
            
            await HubContext.Clients.Clients(clientIds).SendAsync(HubEvents.AttachmentDeleted, channel.Name);
        }

        public async Task OnChangeLastReadMessage(List<Guid> notifyMemberIds, MessageResponse message)
        {
            var channel = await _channelService.GetChannelByIdAsync(message.ChannelId);

            var connectionIds = await GetNotMutedChannelMembersConnectionsAsync(message.ChannelId, notifyMemberIds);
            
            await HubContext.Clients.Clients(connectionIds).SendAsync(HubEvents.LastReadMessageChanged, channel.Name);
        }
    }
}