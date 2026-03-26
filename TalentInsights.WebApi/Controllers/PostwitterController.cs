using Microsoft.AspNetCore.Mvc;
using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.Requests.Post;

namespace TalentInsights.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostwitterController(IPostService postService) : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreatePostRequest model)
    {
        var rsp = postService.Create(model);
        return Ok(rsp);
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] GetAllPostRequest model)
    {
        var rsp = postService.Get(model.Limit ?? 0, model.Offset ?? 0, model.UserId, model.IsPublished);
        return Ok(rsp);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var rsp = postService.Get(id);
        return Ok(rsp);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update([FromBody] UpdatePostRequest model, Guid id)
    {
        var rsp = postService.Update(id, model);
        return Ok(rsp);
    }

    [HttpPatch("change-status/{id:guid}")]
    public IActionResult ChangeStatus(Guid id, [FromBody] ChangePostStatusRequest model)
    {
        var rsp = postService.ChangeStatus(id, model);
        return Ok(rsp);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var rsp = postService.Delete(id);
        return Ok(rsp);
    }
}
