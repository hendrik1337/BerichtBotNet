using BerichtsheftCreator.Berichtsheft;
using BerichtsheftCreator.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();


app.MapGet("/berichtsheft", async (HttpContext httpcontext) =>
{
    Console.WriteLine("Beginning Berichtsheft creation");
    var ausbildungsjahr = httpcontext.Request.Query["ausbildungsjahr"];
    var berichtsheftNummer = httpcontext.Request.Query["berichtsheftNummer"];
    List<Lesson> lessons = await BerichtsheftApiConnector.GetAsync();
    BerichtsheftDocCreator.CreateBerichtsheft(lessons, ausbildungsjahr.ToString(), berichtsheftNummer.ToString());

    return $"Berichtsheft for {ausbildungsjahr.ToString()} Created";
});
app.Run();
