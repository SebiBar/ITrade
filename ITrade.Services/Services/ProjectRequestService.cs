using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class ProjectRequestService
    (
        Context context,
        ICurrentUserService currentUserService
    ) : IProjectRequestService
    {
        public async Task<int> CreateProjectRequestAsync(ProjectRequestReq projectRequest)
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


            var newRequest = new ProjectRequest
            {
                ProjectId = projectRequest.ProjectId,
                SenderId = currentUserService.UserId,
                ReceiverId = projectRequest.ReceiverId,
                ProjectRequestTypeId = (int)projectRequest.RequestType,
                Message = projectRequest.Message,
            };

            await context.ProjectRequests.AddAsync(newRequest);
            await context.SaveChangesAsync();

            return newRequest.Id;
        }

        public async Task DeleteProjectRequestAsync(int projectRequestId)
        {
            var request = await context.ProjectRequests
                .FirstOrDefaultAsync(pr => pr.Id == projectRequestId)
                ?? throw new ArgumentException("Project request not found.", nameof(projectRequestId));

            if (currentUserService.UserId != request.SenderId)
            {
                throw new InvalidOperationException("You can only delete your own requests.");
            }

            context.ProjectRequests.Remove(request);
            await context.SaveChangesAsync();
        }

        public async Task<UserRequestsResponse> GetUserRequestsAsync()
        {
            if (currentUserService.UserRole == UserRoleEnum.Client)
            {
                var invitations = await context.ProjectRequests
                    .Where(pr => pr.SenderId == currentUserService.UserId)
                    .ToListAsync();

                var applications = await context.ProjectRequests
                    .Where(pr => pr.Project.OwnerId == currentUserService.UserId)
                    .ToListAsync();
                // change these to select only userrequestsresponse
            }
            else if (currentUserService.UserRole == UserRoleEnum.Specialist)
            {

            }
            else
            {
                throw new InvalidOperationException("Only clients and specialists can have project requests.");
            }
        }

        public Task ResolveRequestAsync(int projectRequestId, bool accepted)
        {
            throw new NotImplementedException();
        }

        private void ValidateProjectRequestReq(ProjectRequestReq projectRequest)
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

        private async Task EnsureCanSendInvitationAsync(ProjectRequestReq projectRequest)
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
                

            var pendingRequest = await context.ProjectRequests
                .FirstOrDefaultAsync(pr =>
                    pr.ProjectId == projectRequest.ProjectId &&
                    pr.Accepted != true && // pending
                    (
                        (pr.ProjectRequestTypeId == (int)ProjectRequestTypeEnum.Invitation &&
                         pr.ReceiverId == projectRequest.ReceiverId)
                        ||
                        (pr.ProjectRequestTypeId == (int)ProjectRequestTypeEnum.Application &&
                         pr.SenderId == projectRequest.ReceiverId)
                    ));

            if (pendingRequest != null)
            {
                if (pendingRequest.ProjectRequestTypeId == (int)ProjectRequestTypeEnum.Invitation)
                    throw new InvalidOperationException("An invitation has already been sent to this user for this project.");

                throw new InvalidOperationException("The user has already applied to this project.");
            }
        }

        private async Task EnsureCanSendApplicationAsync(ProjectRequestReq projectRequest)
        {
            if (currentUserService.UserRole != UserRoleEnum.Specialist)
            {
                throw new InvalidOperationException("Only specialists can apply to projects.");
            }

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectRequest.ProjectId)
                ?? throw new ArgumentException("Project not found.", nameof(projectRequest.ProjectId));

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

            var pendingRequest = await context.ProjectRequests
                .FirstOrDefaultAsync(pr =>
                    pr.ProjectId == projectRequest.ProjectId &&
                    pr.Accepted != true && // pending
                    (
                        (pr.ProjectRequestTypeId == (int)ProjectRequestTypeEnum.Application &&
                         pr.SenderId == currentUserService.UserId)
                        ||
                        (pr.ProjectRequestTypeId == (int)ProjectRequestTypeEnum.Invitation &&
                         pr.ReceiverId == currentUserService.UserId)
                    ));

            if (pendingRequest != null)
            {
                if (pendingRequest.ProjectRequestTypeId == (int)ProjectRequestTypeEnum.Application)
                    throw new InvalidOperationException("You have already applied to this project.");
                throw new InvalidOperationException("An invitation has already been sent to you for this project.");
            }
        }
    }
}
