using Microsoft.AspNetCore.Mvc;
using TalentInsights.Application.Helpers;
using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.Requests.Collaborator;

namespace TalentInsights.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollaboratorsController(ICollaboratorService collaboratorService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCollaboratorRequest model)
        {
            var rsp = collaboratorService.Create(model);
            return Ok(rsp);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllColaboratorRequest model)
        {
            return Ok(ResponseHelper.Create(collaboratorService.Get(model.Limit ?? 0, model.Offset ?? 0)));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var rsp = collaboratorService.Get(id);
            return Ok(rsp);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromBody] UpdateCollaboratorRequest model, Guid id)
        {
            return Ok($"Usuario actualizado: {id} - {model.FullName}");
        }

        [HttpPatch("change-password/{id:guid}")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordCollaboratorRequest model)
        {
            return Ok($"Usuario contraseña cambiada: {model.CurrentPassword} {model.NewPassword}");
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok($"Usuario eliminado: {id}");
        }
    }
}
