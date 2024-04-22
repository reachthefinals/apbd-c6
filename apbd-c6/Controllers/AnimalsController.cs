using apbd_c6.Models;
using apbd_c6.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apbd_c6.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static readonly List<string> AllowedOrderBy = new List<string>()
        { "name", "description", "category", "area" };

    [HttpGet]
    public IActionResult GetAnimals([FromQuery(Name = "orderBy")] string orderBy = "name")
    {
        if (!AllowedOrderBy.Contains(orderBy)) return BadRequest("Invalid orderBy query param");
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();

        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        // To jest teoretycznie niebezpieczna operacja ale w MSSQL nie możemy robić ORDER BY po wstawionym parametrze.
        // Wartość orderBy jest sprawdzana w pierwszej linijce metody, co zabezpiecza przed SQL injection.
        command.CommandText = $"SELECT * FROM Animal ORDER BY {orderBy} ASC";

        var reader = command.ExecuteReader();

        var animals = new List<Animal>();

        var idAnimalOrdinal = reader.GetOrdinal("IdAnimal");
        var nameOrdinal = reader.GetOrdinal("Name");
        var descOrdinal = reader.GetOrdinal("Description");
        var categoryOrdinal = reader.GetOrdinal("Category");
        var areaOrdinal = reader.GetOrdinal("Area");

        while (reader.Read())
        {
            animals.Add(new Animal()
            {
                IdAnimal = reader.GetInt32(idAnimalOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descOrdinal) ? null : reader.GetString(descOrdinal),
                Category = reader.GetString(categoryOrdinal),
                Area = reader.GetString(areaOrdinal)
            });
        }

        return Ok(animals);
    }
    
    [HttpPost]
    public IActionResult AddAnimal(AddAnimal animal)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT INTO Animal VALUES(@animalName, @animalDesc, @animalCat, @animalArea)";
        command.Parameters.AddWithValue("@animalName", animal.Name);
        command.Parameters.AddWithValue("@animalDesc", animal.Description);
        command.Parameters.AddWithValue("@animalCat", animal.Category);
        command.Parameters.AddWithValue("@animalArea", animal.Area);

        command.ExecuteNonQuery();
        
        return Created("", null);
    }
}