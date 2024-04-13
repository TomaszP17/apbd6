using System.Data.SqlClient;
using Cwiczenia5.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("animals", (IConfiguration configuration) =>
{
    var animals = new List<GetAllAnimalsResponse>();
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("SELECT * FROM Animals", sqlConnection);
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
});

app.Run();