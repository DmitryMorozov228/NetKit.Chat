﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.NetKit.Chat.Domain.TransportModels.Request.Client
{
    public class AddClientRequest
    {
        public AddClientRequest(string saasUserId, string userName, string connectionId, string userAgent)
        {
            SaasUserId = saasUserId;
            UserName = userName;
            ConnectionId = connectionId;
            UserAgent = userAgent;
        }

        public string SaasUserId { get; }

        public string UserName { get; }

        public string ConnectionId { get; }

        public string UserAgent { get; }
    }
}