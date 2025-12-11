var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Authentication
app.MapPost("/MS/IUTADAuth", async (HttpRequest request) =>
{
    var body = await request.ReadFromJsonAsync<LoginRequest>();
    if (body is null) return Results.BadRequest();

    // Simula login com sucesso para qualquer user/pass por agora, ou específico
    if (body.Username == "al55555" || body.Username == "docente1" || body.Username == "dummy") 
    {
        return Results.Ok(new 
        { 
            Username = body.Username, 
            Nome = "Utilizador Dummy " + body.Username, 
            IUPI = "dummy-iupi-" + body.Username 
        });
    }
    
    // Simula falha
    if (body.Username == "erro") return Results.Unauthorized();

    // Default success for easy demo
    return Results.Ok(new 
    { 
        Username = body.Username, 
        Nome = "Utilizador Demo", 
        IUPI = Guid.NewGuid().ToString() 
    });
});

// 2. Get Student IUPI
app.MapGet("/MS/Estudante_IUPI_RCU_S/{username}", (string username) =>
{
    // Return just the string. Minimal API will JSON-encode it (adding quotes).
    // The client code does .Trim('"'), so it expects "value".
    return Results.Ok($"dummy-iupi-{username}");
});

// 3. Get Student Profile by IUPI
app.MapGet("/MS/Estudante_Perfis_RCU_S/{iupi}", (string iupi) =>
{
    // Extrair username do iupi dummy se possivel, senao usar default
    var fakeList = new List<object> 
    {
        new 
        {
            Numero = "55555",
            Nome = "Aluno Exemplo Dummy",
            Email = "al55555@utad.pt",
            OriginalEmail_Deprecated = "al55555@alunos.utad.pt",
            Username_Deprecated = "al55555"
        }
    };
    return Results.Ok(fakeList);
});

// 4. Get Docente info by Username
// Nota: O endpoint real é .../Utilizadores/Get?username=X
app.MapGet("/MS/RCU/v1/Utilizadores/Get", (string username) =>
{
    var docente = new 
    {
        Numero = "12345",
        Nome = "Docente Exemplo",
        Email = $"{username}@utad.pt",
        OriginalEmail_Deprecated = $"{username}@utad.pt",
        Username_Deprecated = username
    };
    return Results.Ok(docente);
});

// 5. Get Courses List
app.MapGet("/MS/SIGAcad/v1/Cursos/List", () =>
{
    var courses = new List<object>
    {
        new { CodigoCurso = 9001, Nome = "Engenharia Informática (Dummy)", SiglaEscola = "ECT" },
        new { CodigoCurso = 9002, Nome = "Engenharia Mecânica (Dummy)", SiglaEscola = "ECT" },
        new { CodigoCurso = 9003, Nome = "Veterinária (Dummy)", SiglaEscola = "ECAV" }
    };
    return Results.Ok(courses);
});

app.Run();

record LoginRequest(string Username, string Password);
