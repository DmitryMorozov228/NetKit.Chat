﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.NetKit.Chat.Domain.Member.TransportModels.Response
{
    public class ParticipantResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public DateTimeOffset? LastNudged { get; set; }
        public UserStatus Status { get; set; }
        public bool IsAfk { get; set; }
        public string SaasUserId { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsBanned { get; set; }
    }
}