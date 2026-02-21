using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class RequestService
    (
        Context context,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IMatchingService matchingService
    ) : IRequestService
    {
        public async Task<int> CreateRequestAsync(RequestReq projectRequest)
        {
            ValidateProjectRequestReq(projectRequest);

            if (projectRequest.RequestType == ProjectRequestTypeEnum.Invitation)
            {
                await EnsureCanSendInvitationAsync(projectRequest);
            }
            else if (projectRequest.RequestType == ProjectRequestTypeEnum.Application)
            {
                await EnsureCanSendApplicationAsync(projectRequest);
            }


            var newRequest = new Request
            {
                ProjectId = projectRequest.ProjectId,
                SenderId = currentUserService.UserId,
                ReceiverId = projectRequest.ReceiverId,
                RequestTypeId = (int)projectRequest.RequestType,
                Message = projectRequest.Message,
            };

            await notificationService.CreateNotificationAsync(
                new NotificationRequest( "New request", "You received a new request", newRequest.ReceiverId));

            await context.Requests.AddAsync(newRequest);
            await context.SaveChangesAsync();

            return newRequest.Id;
        }

        public async Task DeleteRequestAsync(int requestId)
        {
            var request = await context.Requests
                .FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new ArgumentException("Request not found.", nameof(requestId));

            if (currentUserService.UserId != request.SenderId)
            {
                throw new InvalidOperationException("You can only delete your own requests.");
            }

            context.Requests.Remove(request);
            await context.SaveChangesAsync();
        }

        public async Task<bool> UserAlreadyAppliedAsync(int projectId)
        {
            return await context.Requests
                .AnyAsync(r => r.ProjectId == projectId
                    && r.RequestTypeId == (int)ProjectRequestTypeEnum.Application
                    && r.SenderId == currentUserService.UserId
                    && r.Accepted != true);
        }

        public async Task<UserRequestsResponse> GetUserRequestsAsync()
        {
            if (currentUserService.UserRole == UserRoleEnum.Client)
            {
                // Client: sent invitations, received applications on their projects
                var invitations = await context.Requests
                    .Where(r => r.SenderId == currentUserService.UserId 
                        && r.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation)
                    .Select(r => new
                    {
                        Request = new RequestResponse(
                            r.Id,
                            r.Message,
                            r.SenderId,
                            r.Sender.Username,
                            r.ReceiverId,
                            r.Receiver.Username,
                            r.ProjectId,
                            r.Project.Name,
                            ((ProjectRequestTypeEnum)r.RequestTypeId).ToString(),
                            r.Accepted,
                            r.CreatedAt,
                            null
                        ),
                        SpecialistId = r.ReceiverId,
                        r.ProjectId
                    })
                    .ToListAsync();

                var applications = await context.Requests
                    .Where(r => r.Project.OwnerId == currentUserService.UserId 
                        && r.RequestTypeId == (int)ProjectRequestTypeEnum.Application)
                    .Select(r => new
                    {
                        Request = new RequestResponse(
                            r.Id,
                            r.Message,
                            r.SenderId,
                            r.Sender.Username,
                            r.ReceiverId,
                            r.Receiver.Username,
                            r.ProjectId,
                            r.Project.Name,
                            ((ProjectRequestTypeEnum)r.RequestTypeId).ToString(),
                            r.Accepted,
                            r.CreatedAt,
                            null
                        ),
                        SpecialistId = r.SenderId,
                        r.ProjectId
                    })
                    .ToListAsync();

                // Compute match scores in batch
                var allPairs = invitations
                    .Select(i => (i.SpecialistId, i.ProjectId))
                    .Concat(applications.Select(a => (a.SpecialistId, a.ProjectId)))
                    .Distinct()
                    .ToList();

                var scores = await matchingService.ComputeMatchScoresAsync(allPairs);

                var invitationsWithScores = invitations
                    .Select(i => i.Request with { MatchScore = scores.TryGetValue((i.SpecialistId, i.ProjectId), out var s) ? s : 0 })
                    .ToList();

                var applicationsWithScores = applications
                    .Select(a => a.Request with { MatchScore = scores.TryGetValue((a.SpecialistId, a.ProjectId), out var s) ? s : 0 })
                    .ToList();

                return new UserRequestsResponse(invitationsWithScores, applicationsWithScores);
            }
            else if (currentUserService.UserRole == UserRoleEnum.Specialist)
            {
                // Specialist: received invitations, sent applications
                var invitations = await context.Requests
                    .Where(r => r.ReceiverId == currentUserService.UserId 
                        && r.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation)
                    .Select(r => new
                    {
                        Request = new RequestResponse(
                            r.Id,
                            r.Message,
                            r.SenderId,
                            r.Sender.Username,
                            r.ReceiverId,
                            r.Receiver.Username,
                            r.ProjectId,
                            r.Project.Name,
                            ((ProjectRequestTypeEnum)r.RequestTypeId).ToString(),
                            r.Accepted,
                            r.CreatedAt,
                            null
                        ),
                        r.ProjectId
                    })
                    .ToListAsync();

                var applications = await context.Requests
                    .Where(r => r.SenderId == currentUserService.UserId 
                        && r.RequestTypeId == (int)ProjectRequestTypeEnum.Application)
                    .Select(r => new
                    {
                        Request = new RequestResponse(
                            r.Id,
                            r.Message,
                            r.SenderId,
                            r.Sender.Username,
                            r.ReceiverId,
                            r.Receiver.Username,
                            r.ProjectId,
                            r.Project.Name,
                            ((ProjectRequestTypeEnum)r.RequestTypeId).ToString(),
                            r.Accepted,
                            r.CreatedAt,
                            null
                        ),
                        r.ProjectId
                    })
                    .ToListAsync();

                // Compute match scores in batch (specialist is always current user)
                var specialistId = currentUserService.UserId;
                var allPairs = invitations
                    .Select(i => (specialistId, i.ProjectId))
                    .Concat(applications.Select(a => (specialistId, a.ProjectId)))
                    .Distinct()
                    .ToList();

                var scores = await matchingService.ComputeMatchScoresAsync(allPairs);

                var invitationsWithScores = invitations
                    .Select(i => i.Request with { MatchScore = scores.TryGetValue((specialistId, i.ProjectId), out var s) ? s : 0 })
                    .ToList();

                var applicationsWithScores = applications
                    .Select(a => a.Request with { MatchScore = scores.TryGetValue((specialistId, a.ProjectId), out var s) ? s : 0 })
                    .ToList();

                return new UserRequestsResponse(invitationsWithScores, applicationsWithScores);
            }
            else
            {
                throw new InvalidOperationException("Only clients and specialists can have project requests.");
            }
        }

        public async Task ResolveRequestAsync(int requestId, bool accepted)
        {
            var request = await context.Requests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == currentUserService.UserId)
                ?? throw new ArgumentException("Request not found.", nameof(requestId));

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId)
                ?? throw new ArgumentException("Project not found.", nameof(request.ProjectId));

            if (accepted)
            {
                if (request.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation)
                {
                    project.WorkerId = request.ReceiverId;
                }
                else if (request.RequestTypeId == (int)ProjectRequestTypeEnum.Application)
                {
                    if (currentUserService.UserId != project.OwnerId)
                    {
                        throw new InvalidOperationException("Only the project owner can accept applications.");
                    }
                    project.WorkerId = request.SenderId;
                }

                request.Accepted = true;
                project.ProjectStatusTypeId = (int)ProjectStatusTypeEnum.InProgress;
            }
            else
            {
                if (request.RequestTypeId == (int)ProjectRequestTypeEnum.Application)
                {
                    if (currentUserService.UserId != request.Project.OwnerId)
                    {
                        throw new InvalidOperationException("Only the project owner can decline applications.");
                    }
                }
                request.Accepted = false;
            }

            await context.SaveChangesAsync();
        }

        private void ValidateProjectRequestReq(RequestReq projectRequest)
        {
            if (projectRequest.Message != null && projectRequest.Message.Length > 1000)
            {
                throw new ArgumentException("Message must be less than 1000 characters.");
            }
            if (!Enum.IsDefined(typeof(ProjectRequestTypeEnum), projectRequest.RequestType))
            {
                throw new ArgumentException("Invalid request type.", nameof(projectRequest));
            }
        }

        private async Task EnsureCanSendInvitationAsync(RequestReq projectRequest)
        {
            if (currentUserService.UserRole != UserRoleEnum.Client)
            {
                throw new InvalidOperationException("Only clients can send project invitations.");
            }

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectRequest.ProjectId)
                ?? throw new ArgumentException("Project not found.", nameof(projectRequest.ProjectId));

            if (project.OwnerId != currentUserService.UserId)
            {
                throw new InvalidOperationException("Only the project owner can send invitations.");
            }

            if (project.WorkerId == projectRequest.ReceiverId)
            {
                throw new InvalidOperationException("The user is already assigned to this project.");
            }

            if (project.WorkerId != null)
            {
                throw new InvalidOperationException("This project already has a worker assigned.");
            }
                

            var pendingRequest = await context.Requests
                .FirstOrDefaultAsync(r =>
                    r.ProjectId == projectRequest.ProjectId &&
                    r.Accepted != true && // pending
                    (
                        (r.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation &&
                         r.ReceiverId == projectRequest.ReceiverId)
                        ||
                        (r.RequestTypeId == (int)ProjectRequestTypeEnum.Application &&
                         r.SenderId == projectRequest.ReceiverId)
                    ));

            if (pendingRequest != null)
            {
                if (pendingRequest.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation)
                    throw new InvalidOperationException("An invitation has already been sent to this user for this project.");

                throw new InvalidOperationException("The user has already applied to this project.");
            }
        }

        private async Task EnsureCanSendApplicationAsync(RequestReq projectRequest)
        {
            if (currentUserService.UserRole != UserRoleEnum.Specialist)
            {
                throw new InvalidOperationException("Only specialists can apply to projects.");
            }

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectRequest.ProjectId)
                ?? throw new ArgumentException("Project not found.", nameof(projectRequest.ProjectId));

            if (project.ProjectStatusTypeId != (int)ProjectStatusTypeEnum.Hiring)
            {
                throw new InvalidOperationException("You can only apply to hiring projects.");
            }

            if (project.OwnerId == currentUserService.UserId)
            {
                throw new InvalidOperationException("The project owner cannot apply to their own project.");
            }
                
            if (project.WorkerId == currentUserService.UserId)
            {
                throw new InvalidOperationException("You are already assigned to this project.");
            }

            if (project.WorkerId != null)
            {
                throw new InvalidOperationException("This project already has a worker assigned.");
            }

            var pendingRequest = await context.Requests
                .FirstOrDefaultAsync(r =>
                    r.ProjectId == projectRequest.ProjectId &&
                    r.Accepted != true && // pending
                    (
                        (r.RequestTypeId == (int)ProjectRequestTypeEnum.Application &&
                         r.SenderId == currentUserService.UserId)
                        ||
                        (r.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation &&
                         r.ReceiverId == currentUserService.UserId)
                    ));

            if (pendingRequest != null)
            {
                if (pendingRequest.RequestTypeId == (int)ProjectRequestTypeEnum.Application)
                    throw new InvalidOperationException("You have already applied to this project.");
                throw new InvalidOperationException("An invitation has already been sent to you for this project.");
            }
        }
    }
}
