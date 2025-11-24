using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NetRedisASide3.Models;
using NetRedisASide3.Services;

namespace NetRedisASide3.Endpoints;

public static class AssuntoEndpoints
{
    public static void MapAssuntoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assuntos")
            .WithTags("Assuntos")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllAssuntos")
            .WithOpenApi(op => new(op)
            {
                Summary = "Lista todos os assuntos",
                Description = "Retorna uma lista completa de todos os assuntos cadastrados. Utiliza cache distribuído."
            });

        group.MapGet("/{id:int}", GetById)
            .WithName("GetAssuntoById")
            .WithOpenApi(op => new(op)
            {
                Summary = "Busca assunto por ID",
                Description = "Retorna um assunto específico pelo seu identificador."
            });

        group.MapPost("/", Create)
            .WithName("CreateAssunto")
            .WithOpenApi(op => new(op)
            {
                Summary = "Cria novo assunto",
                Description = "Cria um novo assunto no sistema."
            });

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateAssunto")
            .WithOpenApi(op => new(op)
            {
                Summary = "Atualiza assunto",
                Description = "Atualiza os dados de um assunto existente."
            });

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteAssunto")
            .WithOpenApi(op => new(op)
            {
                Summary = "Remove assunto",
                Description = "Remove um assunto do sistema."
            });
    }

    private static async Task<IResult> GetAll(AssuntoService service)
    {
        var assuntos = await service.GetAllAsync();
        return Results.Ok(assuntos);
    }

    private static async Task<IResult> GetById(int id, AssuntoService service)
    {
        var assunto = await service.GetByIdAsync(id);
        return assunto is not null ? Results.Ok(assunto) : Results.NotFound(new { message = $"Assunto com ID {id} não encontrado." });
    }


    private static async Task<IResult> Create(
        [FromBody] Assunto assunto,
        AssuntoService service,
        IValidator<Assunto> validator)
    {
        var validationResult = await validator.ValidateAsync(assunto);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await service.AddAsync(assunto);
        return Results.Created($"/api/assuntos/{created.Id}", created);
    }

    private static async Task<IResult> Update(
        int id,
        [FromBody] Assunto assunto,
        AssuntoService service,
        IValidator<Assunto> validator)
    {
        if (id != assunto.Id)
        {
            return Results.BadRequest(new { message = "ID do assunto não corresponde ao ID da URL." });
        }

        var validationResult = await validator.ValidateAsync(assunto);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var updated = await service.UpdateAsync(assunto);
            return Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> Delete(int id, AssuntoService service)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound(new { message = $"Assunto com ID {id} não encontrado." });
    }
}
