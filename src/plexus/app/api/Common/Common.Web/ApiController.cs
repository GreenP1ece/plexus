using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using FluentResults;

namespace Common.Web;

[ApiController]
[Route("api/[controller]/[action]")]
public abstract class ApiController : ControllerBase
{
    protected const string Id = "{id}";
    protected const string PathSeparator = "/";

    private IMediator? _mediator;

    protected IMediator Mediator
        => _mediator ??= HttpContext
            .RequestServices
            .GetService<IMediator>()!;
    protected async Task<ActionResult<TResult>> Send<TResult>(
        IRequest<TResult> request)
    {
        var result = await Mediator.Send(request);
        return Ok(result);
    }

    protected async Task<ActionResult<TResult>> Send<TResult>(
        IRequest<Result<TResult>> request)
    {
        var result = await Mediator.Send(request);
        if (result.IsFailed)
            return BadRequest(result.Errors);
        return Ok(result.Value);
    }
    protected async Task<ActionResult> Send(
        IRequest<Result> request)
    {
        var result = await Mediator.Send(request);
        if (result.IsFailed)
            return BadRequest(result.Errors);
        return Ok();
    }

    protected async Task<ActionResult> Send(
        IRequest<Stream> request)
    {
        var stream = await Mediator.Send(request);
        var headers = Response.GetTypedHeaders();
        headers.CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(30)
        };
        headers.Expires = new DateTimeOffset(DateTime.UtcNow.AddDays(30));

        return File(stream, "application/octet-stream");
    }
}