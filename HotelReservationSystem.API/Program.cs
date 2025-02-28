using HotelReservationSystem.Core.Interfaces;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("HotelReservationConnection"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;

        options.JsonSerializerOptions.MaxDepth = 64;
    });

// Register Repositories
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Register Services
builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

/* Create the database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    context.Database.Migrate();
}
*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
