using TalentInsights.Application.Models.DTOs;
using TalentInsights.Application.Models.Requests.Collaborator;
using TalentInsights.Application.Models.Responses;

namespace TalentInsights.Application.Interfaces.Services
{
    public interface ICollaboratorService
    {
        public GenericResponse<CollaboratorDto> Create(CreateCollaboratorRequest model);
        public GenericResponse<CollaboratorDto> Update(Guid collaboratorId, UpdateCollaboratorRequest model);
        public GenericResponse<List<CollaboratorDto>> Get(int limit, int offset);
        public GenericResponse<CollaboratorDto?> Get(Guid collaboratorId);
        public GenericResponse<bool> Delete(Guid collaboratorId);
    }
}
