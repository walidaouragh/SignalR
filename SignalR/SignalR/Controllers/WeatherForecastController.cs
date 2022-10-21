using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace SignalR.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private IConfiguration Configuration { get; }

    private readonly IDocumentClient _documentClient;
    private readonly string _databaseId;
    private readonly string _collectionId;

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(IDocumentClient documentClient, IConfiguration configuration,
        ILogger<WeatherForecastController> logger)
    {
        _documentClient = documentClient;
        Configuration = configuration;
        _databaseId = Configuration["DatabaseId"];
        _collectionId = "SignalR";
        _logger = logger;

        BuildCollection().Wait();
    }
    
    [EnableCors("MyPolicy")]
    [HttpGet]
    public IQueryable<WeatherForecast> Get()
    {
        var doc = _documentClient.CreateDocumentQuery<WeatherForecast>(
            UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), new FeedOptions { MaxItemCount = 20 });

        return doc;
    }

    [EnableCors("MyPolicy")]
    [HttpPost]
    public async Task<IActionResult> PostWeather([FromBody] WeatherForecast weatherForecast)
    {
        await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
            weatherForecast);
        return Ok();
    }

    private async Task BuildCollection()
    {
        await _documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId });
        await _documentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_databaseId),
            new DocumentCollection { Id = _collectionId });
    }
}
