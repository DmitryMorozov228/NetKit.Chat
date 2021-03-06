﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Chat.Domain.Client.TransportModels.Response
{
    public class ClientResponse
    {
        public Guid Id { get; set; }
        public string ConnectionClientId { get; set; }
        public string UserName { get; set; }
        public string SaasUserId { get; set; }
    }
}