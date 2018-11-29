﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Softeq.NetKit.Chat.Domain.DomainModels
{
    public class ForwardMessage : IBaseEntity<Guid>, ICreated
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public Guid ChannelId { get; set; }
        public Guid? OwnerId { get; set; }
        public Channel Channel { get; set; }
        public Member Owner { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
