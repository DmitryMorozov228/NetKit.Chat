﻿using EnsureThat;
using Softeq.NetKit.Chat.Data.Interfaces.Repository;
using Softeq.NetKit.Chat.Data.Interfaces.SocketConnection;
using Softeq.NetKit.Chat.Data.Interfaces.UnitOfWork;
using Softeq.NetKit.Chat.Domain.Client.TransportModels.Request;
using Softeq.NetKit.Chat.Domain.Client.TransportModels.Response;
using Softeq.NetKit.Chat.Domain.Member;
using Softeq.NetKit.Chat.Domain.Services.Exceptions;
using Softeq.NetKit.Chat.Domain.Services.Exceptions.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Softeq.NetKit.Chat.Data.Repositories;

namespace Softeq.NetKit.Chat.Domain.Services.Client
{
    internal class DbSocketClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DbSocketClientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ClientResponse> GetOrAddClientAsync(AddClientRequest request)
        {

           var member = await _unitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(request.SaasUserId);
            if (member == null)
            {
                var newMember = new Domain.Member.Member
                {
                    Id = Guid.NewGuid(),
                    Role = UserRole.User,
                    IsAfk = false,
                    IsBanned = false,
                    Status = UserStatus.Active,
                    Name = request.UserName,
                    LastActivity = DateTimeOffset.UtcNow,
                    SaasUserId = request.SaasUserId
                };
                await _unitOfWork.MemberRepository.AddMemberAsync(newMember);
            }

            member = await _unitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(request.SaasUserId);

            var client = await _unitOfWork.ClientRepository.GetClientByConnectionIdAsync(request.ConnectionId);
            if (client != null)
            {
                return client.ToClientResponse(member.SaasUserId);
            }

            client = new Domain.Client.Client
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                ClientConnectionId = request.ConnectionId,
                LastActivity = member.LastActivity,
                LastClientActivity = DateTimeOffset.UtcNow,
                Name = request.UserName,
                UserAgent = request.UserAgent
            };

            await _unitOfWork.ClientRepository.AddClientAsync(client);
            return client.ToClientResponse(member.SaasUserId);
        }

        public async Task DeleteClientAsync(DeleteClientRequest request)
        {
            var client = await _unitOfWork.ClientRepository.GetClientByConnectionIdAsync(request.ClientConnectionId);
            Ensure.That(client).WithException(x => new NotFoundException(new ErrorDto(ErrorCode.NotFound, "Client does not exist.")));
            await _unitOfWork.ClientRepository.DeleteClientAsync(client.Id);
        }

        public async Task UpdateActivityAsync(AddClientRequest request)
        {
            var member = await _unitOfWork.MemberRepository.GetMemberBySaasUserIdAsync(request.SaasUserId);
            member.Status = UserStatus.Active;
            member.LastActivity = DateTimeOffset.UtcNow;

            var client = await _unitOfWork.ClientRepository.GetClientByConnectionIdAsync(request.ConnectionId);
            client.UserAgent = request.UserAgent;
            client.LastActivity = member.LastActivity;
            client.LastClientActivity = DateTimeOffset.UtcNow;

            // Remove any Afk notes.
            if (member.IsAfk)
            {
                member.IsAfk = false;
            }

            await _unitOfWork.MemberRepository.UpdateMemberAsync(member);
            await _unitOfWork.ClientRepository.UpdateClientAsync(client);
        }
    } 
}
