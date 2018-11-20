﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.NetKit.Chat.Infrastructure.SignalR.Hubs.Notifications;
using Softeq.NetKit.Chat.Infrastructure.SignalR.Sockets;

namespace Softeq.NetKit.Chat.Infrastructure.SignalR
{
    public class DIModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterApplicationServices(builder);
            RegisterNotificationServices(builder);
        }

        private static void RegisterApplicationServices(ContainerBuilder builder)
        {
            builder.RegisterType<ChannelSocketService>().As<IChannelSocketService>();
            builder.RegisterType<MessageSocketService>().As<IMessageSocketService>();
            builder.RegisterType<MemberSocketService>().As<IMemberSocketService>();
        }

        private static void RegisterNotificationServices(ContainerBuilder builder)
        {
            builder.RegisterType<ChannelNotificationService>().As<IChannelNotificationService>();
            builder.RegisterType<MessageNotificationService>().As<IMessageNotificationService>();
        }
    }
}