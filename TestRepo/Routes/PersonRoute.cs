using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TestRepo.Routes;

public static class PersonRoute
{
    public static void MapPersonRoute(this RouteGroupBuilder route)
    {
        route.MapGet("/", GetAllPerson);
        route.MapGet("/{id:int}", GetPerson);
        route.MapPost("/add", CreatePerson);
        route.MapDelete("/delete", DeleteList);
        route.MapDelete("/delete/{id:int}", DeletePerson);
        route.MapPatch("/save", UpdatePerson);
        route.MapPut("/active/{id:int}", ActivatePerson);
        route.MapPut("/active", ActivatePeople);
    }

    private static async Task<Results<Ok<int>, BadRequest<string>>> CreatePerson(
        ILogger logger,
        IRepository repository,
        Person person
    )
    {
        var msg = new StringBuilder();
        if (string.IsNullOrEmpty(person.Name))
            msg.Append("Name cannot be null or Empty,");
        if (!string.IsNullOrEmpty(person.Email) && !CompileRegex.VerifyEmail(person.Email))
            msg.Append("Wrong format Email,");
        if (msg.Length > 0)
            return TypedResults.BadRequest(msg.ToString().TrimEnd(','));
        await using var transaction = await repository.BeginTransactionAsync();
        try
        {
            await repository.AddAsync(person);
            await repository.SaveChangesAsync();
            await transaction.CommitAsync();
            return TypedResults.Ok(person.Id);
        }
        catch (Exception ex)
        {
            var reason = ex.GetBaseException().Message;
            await transaction.RollbackAsync();
            logger.WriteToDatabaseFail(nameof(Person), reason);
            return TypedResults.BadRequest(reason);
        }
    }

    private static async Task<Results<Ok<int>, NotFound<string>>> DeletePerson(
        ILogger logger,
        IRepository repository,
        int id,
        bool force = false
    )
    {
        try
        {
            var person = await repository.GetAsync<Person>(p => p.Id == id);
            if (person is null)
                return TypedResults.NotFound("id");
            if (force)
                repository.Remove(person);
            else
            {
                person.IsDeleted = true;
                repository.Update(person);
            }

            await repository.SaveChangesAsync();
            return TypedResults.Ok(id);
        }
        catch (Exception ex)
        {
            logger.WriteToDatabaseFail(nameof(Person), ex.GetBaseException().Message);
            return TypedResults.NotFound(nameof(Person));
        }
    }

    private static async Task<Results<Ok<int>, NotFound<string>>> DeleteList(
        ILogger logger,
        IRepository repository,
        MyAppContext context,
        [FromBody] int[] peopleId,
        bool force = false
    )
    {
        try
        {
            var people = await repository.GetListAsync<Person>(p => peopleId.Contains(p.Id), false);
            if (people.Count == 0)
                return TypedResults.NotFound("id");
            if (force)
                await repository.ExecuteDeleteAsync<Person>(p => peopleId.Contains(p.Id));
            else
                await context.BulkUpdateAsync(
                    people.Select(p =>
                    {
                        p.IsDeleted = true;
                        return p;
                    })
                );
            return TypedResults.Ok(peopleId[0]);
        }
        catch (Exception ex)
        {
            logger.WriteToDatabaseFail(nameof(Person), ex.GetBaseException().Message);
            return TypedResults.NotFound(nameof(Person));
        }
    }

    private static async Task<Results<JsonHttpResult<Person>, NotFound<string>>> GetPerson(
        ILogger logger,
        IRepository repository,
        int id
    )
    {
        try
        {
            var res = await repository.GetAsync<Person>(p => p.Id == id && !p.IsDeleted, true);
            return TypedResults.Json(res, PersonSerializer.Default.Person);
        }
        catch (Exception ex)
        {
            logger.ReadFromDatabaseFail(nameof(Person), ex.GetBaseException().Message);
            return TypedResults.NotFound(nameof(Person));
        }
    }

    private static async Task<Results<JsonHttpResult<List<Person>>, NotFound<string>>> GetAllPerson(
        ILogger logger,
        IRepository repository
    )
    {
        try
        {
            var spec = new Specification<Person> { OrderBy = p => p.OrderBy(t => t.Id) };
            var res = await repository.GetListAsync(spec, true);
            return TypedResults.Json(res, PersonSerializer.Default.ListPerson);
        }
        catch (Exception ex)
        {
            logger.ReadFromDatabaseFail(nameof(Person), ex.GetBaseException().Message);
            return TypedResults.NotFound(nameof(Person));
        }
    }

    private static async Task<Results<Ok<int>, BadRequest<string>>> UpdatePerson(
        ILogger logger,
        IRepository repository,
        Person person
    )
    {
        var msg = new StringBuilder();
        if (string.IsNullOrEmpty(person.Name))
            msg.Append("Name cannot be null or Empty,");
        if (!string.IsNullOrEmpty(person.Email) && !CompileRegex.VerifyEmail(person.Email))
            msg.Append("Wrong format Email,");
        if (person.Id == 0)
            msg.Append("Id cannot be zero,");
        if (msg.Length > 0)
            return TypedResults.BadRequest(msg.ToString().TrimEnd(','));
        await using var transaction = await repository.BeginTransactionAsync();
        try
        {
            repository.Update(person);
            await repository.SaveChangesAsync();
            await transaction.CommitAsync();
            return TypedResults.Ok(person.Id);
        }
        catch (Exception ex)
        {
            var reason = ex.GetBaseException().Message;
            await transaction.RollbackAsync();
            logger.WriteToDatabaseFail(nameof(Person), reason);
            return TypedResults.BadRequest(reason);
        }
    }

    private static async Task<Results<Ok<int>, BadRequest<string>>> ActivatePerson(
        ILogger logger,
        IRepository repository,
        int id
    )
    {
        try
        {
            var person = await repository.GetByIdAsync<Person>(id);
            if (person is null)
                return TypedResults.BadRequest("No person found");
            person.IsDeleted = false;
            repository.Update(person);
            await repository.SaveChangesAsync();
            return TypedResults.Ok(id);
        }
        catch (Exception ex)
        {
            var reason = ex.GetBaseException().Message;
            logger.WriteToDatabaseFail(nameof(Person), reason);
            return TypedResults.BadRequest(reason);
        }
    }

    private static async Task<Results<Ok<int>, BadRequest<string>>> ActivatePeople(
        ILogger logger,
        IRepository repository,
        MyAppContext context,
        [FromBody] int[] ids
    )
    {
        try
        {
            var people = await repository.GetListAsync<Person>(p => ids.Contains(p.Id), false);
            if (people.Count == 0)
                return TypedResults.BadRequest("No person found");
            await context.BulkUpdateAsync(
                people.Select(p =>
                {
                    p.IsDeleted = false;
                    return p;
                })
            );
            return TypedResults.Ok(ids[0]);
        }
        catch (Exception ex)
        {
            var reason = ex.GetBaseException().Message;
            logger.WriteToDatabaseFail(nameof(Person), reason);
            return TypedResults.BadRequest(reason);
        }
    }
}

internal static partial class CompileRegex
{
    [GeneratedRegex(
        @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex EmailRegex();

    public static bool VerifyEmail(string email) => EmailRegex().IsMatch(email);
}

[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(List<Person>))]
internal partial class PersonSerializer : JsonSerializerContext;
