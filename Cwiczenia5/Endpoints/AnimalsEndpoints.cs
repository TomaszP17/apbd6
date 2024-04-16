using System.Data.SqlClient;
using Cwiczenia5.DTOs;
using FluentValidation;

namespace Cwiczenia5.Endpoints;

public static class AnimalsEndpoints
{
    public static void RegisterAnimalsEndpoints(this WebApplication app)
    {
        var animals = app.MapGroup("api");

        animals.MapGet("/animals", GetAnimals);
        animals.MapGet("/animals/{id}", GetAnimal);
        animals.MapPost("/animals", CreateAnimal);
        animals.MapPut("/animals/{id}", ReplaceAnimal);
        animals.MapDelete("/animals/{id}", DeleteAnimal);
    }
    private static IResult GetAnimals(IConfiguration configuration, string? orderBy = "")
    {
        var animals = new List<GetAllAnimalsResponse>();
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var orderColumnsName = new List<string> {"IdAnimal","Name", "Description", "Category", "Area"};
            string chosenColumn;
            if (string.IsNullOrEmpty(orderBy))
            {
                chosenColumn = "Name";
            }
            else if (orderColumnsName.Contains(orderBy))
            {
                chosenColumn = orderBy;
            }
            else
            {
                return Results.NotFound("Nie ma takiej kolumny po ktorej bys chcial sortowac!");
            }
            var sqlCommand = new SqlCommand($"SELECT * FROM Animals ORDER BY {chosenColumn}", sqlConnection);
            sqlCommand.Connection.Open();
            var reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                animals.Add(new GetAllAnimalsResponse(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4)));
            }
        }
        return Results.Ok(animals);
    }
    private static IResult GetAnimal(IConfiguration configuration, int id)
    {
        using var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("SELECT * FROM Animals WHERE IdAnimal = @id", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();
        var reader = sqlCommand.ExecuteReader();

        if (!reader.Read()) return Results.NotFound();
                
        return Results.Ok(new GetAllAnimalsResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4))
        );
    }
    private static IResult CreateAnimal(IConfiguration configuration, CreateAnimalRequest animalRequest, IValidator<CreateAnimalRequest> validator)
    {
        var validation = validator.Validate(animalRequest);
        if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());


        using var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("INSERT INTO Animals (Name, Description, Category, Area)" +
                                        " values (@n, @d, @c, @a)", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@n", animalRequest.Name);
        sqlCommand.Parameters.AddWithValue("@d", animalRequest.Description);
        sqlCommand.Parameters.AddWithValue("@c", animalRequest.Category);
        sqlCommand.Parameters.AddWithValue("@a", animalRequest.Area);
        sqlCommand.Connection.Open();
        sqlCommand.ExecuteNonQuery();

        return Results.Created("", null);
    }
    private static IResult ReplaceAnimal(IConfiguration configuration, IValidator<CreateAnimalRequest> validator,
        int id, CreateAnimalRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

        using var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("UPDATE Animals SET Name = @n, Description = @d, Category = @c," +
                                        " Area = @a WHERE IdAnimal = @id", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@n", request.Name);
        sqlCommand.Parameters.AddWithValue("@d", request.Description);
        sqlCommand.Parameters.AddWithValue("@c", request.Category);
        sqlCommand.Parameters.AddWithValue("@a", request.Area);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();
            
        var affectedRows = sqlCommand.ExecuteNonQuery();
        return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
    }
    private static IResult DeleteAnimal(IConfiguration configuration, int id)
    {
        using var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("DELETE FROM Animals WHERE IdAnimal = @id", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();
            
        var affectedRows = sqlCommand.ExecuteNonQuery();
        return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
    }
}