﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Chat.Domain.TransportModels.Request.Channel
{
    public class JoinToChannelRequest
    {
        public JoinToChannelRequest(string saasUserId, Guid channelId)
        {
            SaasUserId = saasUserId;
            ChannelId = channelId;
        }

        public string SaasUserId { get; }

        public Guid ChannelId { get; }
    }
}