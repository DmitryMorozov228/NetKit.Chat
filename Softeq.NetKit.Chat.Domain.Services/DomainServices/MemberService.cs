﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using EnsureThat;
using Softeq.NetKit.Chat.Data.Cloud.DataProviders;
using Softeq.NetKit.Chat.Data.Persistent;
using Softeq.NetKit.Chat.Domain.DomainModels;
using Softeq.NetKit.Chat.Domain.Exceptions;
using Softeq.NetKit.Chat.Domain.Services.Mappers;
using Softeq.NetKit.Chat.Domain.TransportModels.Request.Member;
using Softeq.NetKit.Chat.Domain.TransportModels.Response.Channel;
using Softeq.NetKit.Chat.Domain.TransportModels.Response.Client;
using Softeq.NetKit.Chat.Domain.TransportModels.Response.Member;

namespace Softeq.NetKit.Chat.Domain.Services.DomainServices
{
    internal class MemberService : BaseService, IMemberService
    {
        private readonly ICloudImageProvider _cloudImageProvider;

        public MemberService(IUnitOfWork unitOfWork, ICloudImageProvider cloudImageProvider)
            : base(unitOfWork)
        {
            Ensure.That(cloudImageProvider).IsNotNull();

            _cloudImageProvider = cloudImageProvider;
        }

        public async Task<MemberSummary> GetMemberBySaasUserIdAsync(string saasUserId)
        {
            var member = await UnitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(saasUserId);
            if (member == null)
            {
                throw new NetKitChatNotFoundException($"Unable to get member by {nameof(saasUserId)}. Member {nameof(saasUserId)}:{saasUserId} not found.");
            }

            var memberAvatarUrl = _cloudImageProvider.GetMemberAvatarUrl(member.PhotoName);
            return member.ToMemberSummary(memberAvatarUrl);
        }

        public async Task<MemberSummary> GetMemberByIdAsync(Guid memberId)
        {
            var member = await UnitOfWork.MemberRepository.GetMemberByIdAsync(memberId);
            if (member == null)
            {
                throw new NetKitChatNotFoundException($"Unable to get member by {nameof(memberId)}. Member {nameof(memberId)}:{memberId} not found.");
            }

            var memberAvatarUrl = _cloudImageProvider.GetMemberAvatarUrl(member.PhotoName);
            return member.ToMemberSummary(memberAvatarUrl);
        }

        public async Task<IReadOnlyCollection<MemberSummary>> GetChannelMembersAsync(Guid channelId)
        {
            var channel = await UnitOfWork.ChannelRepository.GetChannelByIdAsync(channelId);
            if (channel == null)
            {
                throw new NetKitChatNotFoundException($"Unable to get channel members. Channel {nameof(channelId)}:{channelId} not found.");
            }

            var members = await UnitOfWork.MemberRepository.GetAllMembersByChannelIdAsync(channelId);
            return members.Select(x =>
            {
                var memberAvatarUrl = _cloudImageProvider.GetMemberAvatarUrl(x.PhotoName);
                return x.ToMemberSummary(memberAvatarUrl);
            }).ToList().AsReadOnly();
        }

        public async Task<ChannelResponse> InviteMemberAsync(Guid memberId, Guid channelId)
        {
            var channel = await UnitOfWork.ChannelRepository.GetChannelByIdAsync(channelId);
            if (channel == null)
            {
                throw new NetKitChatNotFoundException($"Unable to invite member. Channel {nameof(channelId)}:{channelId} not found.");
            }

            if (channel.IsClosed)
            {
                throw new NetKitChatInvalidOperationException($"Unable to invite member. Channel {nameof(channelId)}:{channelId} is closed.");
            }

            var member = await UnitOfWork.MemberRepository.GetMemberByIdAsync(memberId);
            if (member == null)
            {
                throw new NetKitChatNotFoundException($"Unable to invite member. Member {nameof(memberId)}:{memberId} not found.");
            }

            var isMemberExistsInChannel = await UnitOfWork.ChannelRepository.IsMemberExistsInChannelAsync(member.Id, channel.Id);
            if (isMemberExistsInChannel)
            {
                throw new NetKitChatInvalidOperationException($"Unable to invite member. Member {nameof(memberId)}:{memberId} already joined channel {nameof(channelId)}:{channelId}.");
            }

            var channelMember = new ChannelMembers
            {
                ChannelId = channel.Id,
                MemberId = member.Id,
                LastReadMessageId = null
            };

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UnitOfWork.ChannelMemberRepository.AddChannelMemberAsync(channelMember);
                await UnitOfWork.ChannelRepository.IncrementChannelMembersCount(channel.Id);

                transactionScope.Complete();
            }

            channel = await UnitOfWork.ChannelRepository.GetChannelByIdAsync(channel.Id);

            return channel.ToChannelResponse();
        }

        public async Task<MemberSummary> AddMemberAsync(string saasUserId, string email)
        {
            var member = await UnitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(saasUserId);
            if (member != null)
            {
                throw new NetKitChatInvalidOperationException($"Unable to add member. Member {nameof(saasUserId)}:{saasUserId} already exists.");
            }

            var newMember = new Member
            {
                Id = Guid.NewGuid(),
                Role = UserRole.User,
                IsAfk = false,
                IsBanned = false,
                Status = UserStatus.Active,
                SaasUserId = saasUserId,
                Email = email,
                LastActivity = DateTimeOffset.UtcNow,
                Name = email
            };
            await UnitOfWork.MemberRepository.AddMemberAsync(newMember);

            var newMemberAvatarUrl = _cloudImageProvider.GetMemberAvatarUrl(newMember.PhotoName);
            return newMember.ToMemberSummary(newMemberAvatarUrl);
        }

        public async Task<IReadOnlyCollection<Client>> GetMemberClientsAsync(Guid memberId)
        {
            var clients = await UnitOfWork.ClientRepository.GetMemberClientsAsync(memberId);
            return clients.ToList().AsReadOnly();
        }

        public async Task UpdateMemberStatusAsync(string saasUserId, UserStatus status)
        {
            var member = await UnitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(saasUserId);
            if (member == null)
            {
                throw new NetKitChatNotFoundException($"Unable to update member status. Member {nameof(saasUserId)}:{saasUserId} not found.");
            }

            member.Status = status;
            member.LastActivity = DateTimeOffset.Now;
            await UnitOfWork.MemberRepository.UpdateMemberAsync(member);
        }

        public async Task<IReadOnlyCollection<ClientResponse>> GetClientsByMemberIds(List<Guid> memberIds)
        {
            var clients = await UnitOfWork.ClientRepository.GetClientsByMemberIdsAsync(memberIds);
            return clients.Select(x => x.ToClientResponse()).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyCollection<MemberSummary>> GetAllMembersAsync()
        {
            var members = await UnitOfWork.MemberRepository.GetAllMembersAsync();
            return members.Select(x =>
            {
                var memberAvatarUrl = _cloudImageProvider.GetMemberAvatarUrl(x.PhotoName);
                return x.ToMemberSummary(memberAvatarUrl);
            }).ToList().AsReadOnly();
        }

        public async Task UpdateActivityAsync(UpdateMemberActivityRequest request)
        {
            var member = await UnitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(request.SaasUserId);
            member.Status = UserStatus.Active;
            member.LastActivity = DateTimeOffset.UtcNow;

            var client = await UnitOfWork.ClientRepository.GetClientByConnectionIdAsync(request.ConnectionId);
            client.UserAgent = request.UserAgent;
            client.LastActivity = member.LastActivity;
            client.LastClientActivity = DateTimeOffset.UtcNow;

            // Remove any Afk notes.
            if (member.IsAfk)
            {
                member.IsAfk = false;
            }

            await UnitOfWork.MemberRepository.UpdateMemberAsync(member);
            await UnitOfWork.ClientRepository.UpdateClientAsync(client);
        }
    }
}