using Minstrel.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMinstrelServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();