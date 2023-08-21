using Microsoft.OpenApi.Models;
using APIContagem;
using APIContagem.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "APIContagem",
            Description = "Exemplo de implementação de Minimal API para Contagem de acessos", 
            Version = "v1",
            Contact = new OpenApiContact()
            {
                Name = "Renato Groffe",
                Url = new Uri("https://github.com/renatogroffe"),
            },
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("http://opensource.org/licenses/MIT"),
            }
        });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization Header - utilizado com Bearer Authentication.\r\n\r\n" +
            "Digite 'Bearer' [espaço] e então seu token no campo abaixo.\r\n\r\n" +
            "Exemplo (informar sem as aspas): 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();

builder.Services.AddSingleton<Contador>();

builder.Services.AddCors();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIContagem v1");
});

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/contador", (Contador contador) =>
{
    int valorAtualContador;
    lock (contador)
    {
        contador.Incrementar();
        valorAtualContador = contador.ValorAtual;
    }
    app.Logger.LogInformation($"Contador - Valor atual: {valorAtualContador}");    

    return TypedResults.Ok(new ResultadoContador()
    {
        ValorAtual = contador.ValorAtual,
        Local = contador.Local,
        Kernel = contador.Kernel,
        Framework = contador.Framework,
        Mensagem = app.Configuration["Saudacao"]
    });
}).Produces<ResultadoContador>()
  .Produces(StatusCodes.Status401Unauthorized)
  .RequireAuthorization();

app.Run();