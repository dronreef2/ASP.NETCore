using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o serviço do YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
// Removido Swagger para manter o gateway minimalista

app.UseHttpsRedirection();

// Mapeamento de rotas para o proxy no pipeline
app.MapReverseProxy();

app.Run();
