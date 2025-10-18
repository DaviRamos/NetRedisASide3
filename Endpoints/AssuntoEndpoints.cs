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

    private static async Task<IResult> Create(
        [FromBody] Movimentacao movimentacao,
        MovimentacaoService service,
        IValidator<Movimentacao> validator)
    {
        var validationResult = await validator.ValidateAsync(movimentacao);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await service.AddAsync(movimentacao);
        return Results.Created($"/api/movimentacoes/{created.Id}", created);
    }

    private static async Task<IResult> Update(
        int id,
        [FromBody] Movimentacao movimentacao,
        MovimentacaoService service,
        IValidator<Movimentacao> validator)
    {
        if (id != movimentacao.Id)
        {
            return Results.BadRequest(new { message = "ID da movimentação não corresponde ao ID da URL." });
        }

        var validationResult = await validator.ValidateAsync(movimentacao);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var updated = await service.UpdateAsync(movimentacao);
            return Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> Delete(int id, MovimentacaoService service)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound(new { message = $"Movimentação com ID {id} não encontrada." });
    }
}
