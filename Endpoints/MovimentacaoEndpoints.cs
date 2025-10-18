using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NetRedisASide3.Models;
using NetRedisASide3.Services;

namespace NetRedisASide3.Endpoints;

public static class MovimentacaoEndpoints
{
    public static void MapMovimentacaoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movimentacoes")
            .WithTags("Movimentações")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllMovimentacoes")
            .WithOpenApi(op => new(op)
            {
                Summary = "Lista todas as movimentações",
                Description = "Retorna uma lista completa de todas as movimentações cadastradas. Utiliza cache distribuído."
            });

        group.MapGet("/{id:int}", GetById)
            .WithName("GetMovimentacaoById")
            .WithOpenApi(op => new(op)
            {
                Summary = "Busca movimentação por ID",
                Description = "Retorna uma movimentação específica pelo seu identificador."
            });

        group.MapPost("/", Create)
            .WithName("CreateMovimentacao")
            .WithOpenApi(op => new(op)
            {
                Summary = "Cria nova movimentação",
                Description = "Cria uma nova movimentação no sistema."
            });

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateMovimentacao")
            .WithOpenApi(op => new(op)
            {
                Summary = "Atualiza movimentação",
                Description = "Atualiza os dados de uma movimentação existente."
            });

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteMovimentacao")
            .WithOpenApi(op => new(op)
            {
                Summary = "Remove movimentação",
                Description = "Remove uma movimentação do sistema."
            });
    }

    private static async Task<IResult> GetAll(MovimentacaoService service)
    {
        var movimentacoes = await service.GetAllAsync();
        return Results.Ok(movimentacoes);
    }

    private static async Task<IResult> GetById(int id, MovimentacaoService service)
    {
        var movimentacao = await service.GetByIdAsync(id);
        return movimentacao is not null ? Results.Ok(movimentacao) : Results.NotFound(new { message = $"Movimentação com ID {id} não encontrada." });
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

