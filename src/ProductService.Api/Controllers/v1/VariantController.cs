﻿using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Variants.Commands.Create;
using ProductService.Application.Features.Variants.Queries.GetList;
using SharedLibrary.Dtos.Variants;

namespace ProductService.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class VariantController : ControllerBase
{
    private readonly ISender _sender;
    public VariantController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("GetVariantListByProductId")]
    public async Task<IActionResult> GetVariantListByProductId([FromQuery] ListVariantsQuery model)
    {
        var query = new ListVariantsQuery(model.ProductId, model.Filter);
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpPost("CreateVariant")]
    public async Task<IActionResult> CreateVariant([FromBody] CreateVariantDto model)
    {
        var command = new CreateVariantCommand(model);
        var result = await _sender.Send(command);
        return Ok(result);
    }

}
