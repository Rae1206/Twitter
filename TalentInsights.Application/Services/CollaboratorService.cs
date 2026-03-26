using TalentInsights.Application.Helpers;
using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.DTOs;
using TalentInsights.Application.Models.Requests.Collaborator;
using TalentInsights.Application.Models.Responses;
using TalentInsights.Shared;
using TalentInsights.Shared.Helpers;

namespace TalentInsights.Application.Services
{
    public class CollaboratorService(Cache<CollaboratorDto> cache) : ICollaboratorService
    {
        public GenericResponse<CollaboratorDto> Create(CreateCollaboratorRequest model)
        {
            var collaborator = new CollaboratorDto
            {
                CollaboratorId = Guid.NewGuid(),
                FullName = model.FullName,
                GitlabProfile = model.GitlabProfile,
                Position = model.Position,
                CreatedAt = DateTimeHelper.UtcNow(),
                JoinedAt = DateTimeHelper.UtcNow()
            };

            cache.Add(collaborator.CollaboratorId.ToString(), collaborator);

            return ResponseHelper.Create(collaborator);
        }

        public GenericResponse<bool> Delete(Guid collaboratorId)
        {
            var siExiste = cache.Get(collaboratorId.ToString());

            if (siExiste is null)
            {
                return ResponseHelper.Create(false);
            }

            cache.Delete(collaboratorId.ToString());
            return ResponseHelper.Create(true);
        }

        public GenericResponse<List<CollaboratorDto>> Get(int limit, int offset)
        {
            var colaboradores = cache.Get();
            return ResponseHelper.Create(colaboradores);
        }

        public GenericResponse<CollaboratorDto?> Get(Guid collaboratorId)
        {
            var colaborador = cache.Get(collaboratorId.ToString());
            return ResponseHelper.Create(colaborador);
        }

        public GenericResponse<CollaboratorDto> Update(Guid collaboratorId, UpdateCollaboratorRequest model)
        {
            throw new NotImplementedException();
        }
    }
}
