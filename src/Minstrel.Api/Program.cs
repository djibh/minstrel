using Minstrel.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMinstrelServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevOpen", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DevOpen");
// app.UseHttpsRedirection();
app.MapControllers();

// Temporary
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();