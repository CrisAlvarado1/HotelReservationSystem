using HotelReservationSystem.Core.Interfaces;
using HotelReservationSystem.Core.Services;
using HotelReservationSystem.Infrastructure.Data;
using HotelReservationSystem.Infrastructure.Interfaces;
using HotelReservationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

// Register Repositories
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

// Register Services
builder.Services.AddScoped<IRoomService, RoomService>();

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
