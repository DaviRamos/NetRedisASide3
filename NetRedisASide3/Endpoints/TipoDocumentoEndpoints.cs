// Endpoints/TipoDocumentoEndpoints.cs
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NetRedisASide3.Models;
using NetRedisASide3.Services;

namespace NetRedisASide3.Endpoints;

public static class TipoDocumentoEndpoints
{
    public static void MapTipoDocumentoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tipos-documento")
            .WithTags("Tipos de Documento")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTiposDocumento")
            .WithOpenApi(op => new(op)
            {
                Summary = "Lista todos os tipos de documento",
                Description = "Retorna uma lista completa de todos os tipos de documento cadastrados. Utiliza cache distribuído."
            });

        group.MapGet("/{id:int}", GetById)
            .WithName("GetTipoDocumentoById")
            .WithOpenApi(op => new(op)
            {
                Summary = "Busca tipo de documento por ID",
                Description = "Retorna um tipo de documento específico pelo seu identificador."
            });

        group.MapPost("/", Create)
            .WithName("CreateTipoDocumento")
            .WithOpenApi(op => new(op)
            {
                Summary = "Cria novo tipo de documento",
                Description = "Cria um novo tipo de documento no sistema."
            });

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateTipoDocumento")
            .WithOpenApi(op => new(op)
            {
                Summary = "Atualiza tipo de documento",
                Description = "Atualiza os dados de um tipo de documento existente."
            });

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteTipoDocumento")
            .WithOpenApi(op => new(op)
            {
                Summary = "Remove tipo de documento",
                Description = "Remove um tipo de documento do sistema."
            });
    }

    private static async Task<IResult> GetAll(TipoDocumentoService service)
    {
        var tipos = await service.GetAllAsync();
        return Results.Ok(tipos);
    }

    private static async Task<IResult> GetById(int id, TipoDocumentoService service)
    {
        var tipo = await service.GetByIdAsync(id);
        return tipo is not null ? Results.Ok(tipo) : Results.NotFound(new { message = $"Tipo de documento com ID {id} não encontrado." });
    }

    private static async Task<IResult> Create(
        [FromBody] TipoDocumento tipo,
        TipoDocumentoService service,
        IValidator<TipoDocumento> validator)
    {
        var validationResult = await validator.ValidateAsync(tipo);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await service.AddAsync(tipo);
        return Results.Created($"/api/tipos-documento/{created.Id}", created);
    }

    private static async Task<IResult> Update(
        int id,
        [FromBody] TipoDocumento tipo,
        TipoDocumentoService service,
        IValidator<TipoDocumento> validator)
    {
        if (id != tipo.Id)
        {
            return Results.BadRequest(new { message = "ID do tipo de documento não corresponde ao ID da URL." });
        }

        var validationResult = await validator.ValidateAsync(tipo);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var updated = await service.UpdateAsync(tipo);
            return Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> Delete(int id, TipoDocumentoService service)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound(new { message = $"Tipo de documento com ID {id} não encontrado." });
    }
} 