using LoncotesLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddNpgsql<LoncotesLibraryDbContext>(builder.Configuration["LoncotesLibraryDbConnectionString"]);

builder.Services.Configure<JsonOptions>(options => 
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/materials", (LoncotesLibraryDbContext db, int materialTypeId, int genreId) =>
{
    if (materialTypeId == null && genreId == null){
        return db.Materials.Include(m => m.MaterialType).Include(m => m.Genre).Where(m => m.OutOfCirculationSince == null).ToList();
    }
    else if (materialTypeId == null && genreId != null){
        return db.Materials.Include(m => m.MaterialType).Include(m => m.Genre).Where(m => m.OutOfCirculationSince == null && m.GenreId == genreId).ToList();
    }
    else if (materialTypeId != null && genreId == null) {
        return db.Materials.Include(m => m.MaterialType).Include(m => m.Genre).Where(m => m.OutOfCirculationSince == null && m.MaterialTypeId == materialTypeId).ToList();
    }
    else {
        return db.Materials.Include(m => m.MaterialType).Include(m => m.Genre).Where(m => m.OutOfCirculationSince == null && m.MaterialTypeId == materialTypeId && m.GenreId == genreId).ToList();
    }
    
});

//get material by id
app.MapGet("/api/materials/{id}", (LoncotesLibraryDbContext db,int id) =>
{
    return db.Materials.Include(m => m.MaterialType).Include(m => m.Genre).Include(m => m.Checkouts).ThenInclude(m => m.Patron).SingleOrDefault(m => m.Id == id);
});

//add material
app.MapPost("/api/materials", (LoncotesLibraryDbContext db, Material material) =>
{
    db.Materials.Add(material);
    db.SaveChanges();
    return Results.Created($"/api/materials/{material.Id}", material);
});

//soft delete material with setting outofcirculation to datetime.now
app.MapPut("/api/materials/{id}", (LoncotesLibraryDbContext db, int id) => 
{
    Material materialToUpdate = db.Materials.SingleOrDefault(m => m.Id == id);
    if (materialToUpdate == null)
    {
        return Results.NotFound();
    }
    materialToUpdate.OutOfCirculationSince = DateTime.Now;
    db.SaveChanges();
    return Results.NoContent();
});

//get materialtypes
app.MapGet("/api/materialtypes", (LoncotesLibraryDbContext db) => 
{
    return db.MaterialTypes.ToList();
});

//get genres
app.MapGet("/api/genres", (LoncotesLibraryDbContext db)=> {
    return db.Genres.ToList();
});

//get patrons
app.MapGet("/api/patrons", (LoncotesLibraryDbContext db)=> {
    return db.Patrons.ToList();
});

//get patron by id with checkouts materials and types
app.MapGet("/api/patrons/{id}", (LoncotesLibraryDbContext db, int id)=>
{
    return db.Patrons.Include(p => p.Checkouts).ThenInclude(c => c.Material).ThenInclude(m => m.MaterialType).SingleOrDefault(p => p.Id == id);
});

//update patron email and address
app.MapPut("/api/patrons/{id}", (LoncotesLibraryDbContext db, int id, Patron patron)=>
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(p => p.Id == id);
    if (patronToUpdate == null){
        return Results.NotFound();
    }
    patronToUpdate.Address = patron.Address;
    patronToUpdate.Email = patron.Email;

    db.SaveChanges();
    return Results.NoContent();
});

//soft delete patron by changing isactive to false
app.MapPut("/api/patrons/left/{id}", (LoncotesLibraryDbContext db, int id)=> 
{
    Patron patronToUpdate = db.Patrons.SingleOrDefault(p => p.Id == id);
    if (patronToUpdate == null){
        return Results.NotFound();
    }
    patronToUpdate.IsActive = false;
    db.SaveChanges();
    return Results.NoContent();
});

//checkout material, make new checkout for material and patron, set checkout date to datetime.today
app.MapPost("/api/checkouts", (LoncotesLibraryDbContext db, Checkout checkout)=>
{
    checkout.Id = db.Checkouts.Max(c=>c.Id) + 1;
    checkout.CheckoutDate = DateTime.Today;
    db.Checkouts.Add(checkout);
    db.SaveChanges();
    return Results.Created($"/api/checkout/{checkout.Id}", checkout);
});

//return material, update checkout by id, change returndate to datetime.today
app.MapPut("/api/checkouts/{id}", (LoncotesLibraryDbContext db, int id)=> 
{
    Checkout checkout = db.Checkouts.SingleOrDefault(c => c.Id == id);
    if (checkout == null){
        return Results.NotFound();
    }
    checkout.ReturnDate = DateTime.Today;
    db.SaveChanges();
    return Results.NoContent();
});

//get available materials, return list of materials who don't have a checkout (which is a list of checkout objects) with null return date
app.MapGet("/materials/available", (LoncotesLibraryDbContext db) =>
{
    return db.Materials
    .Where(m => m.OutOfCirculationSince == null)
    .Where(m => m.Checkouts.All(co => co.ReturnDate != null))
    .ToList();
});

//get overdue checkouts
app.MapGet("/checkouts/overdue", (LoncotesLibraryDbContext db) =>
{
    return db.Checkouts
    .Include(p => p.Patron)
    .Include(co => co.Material)
    .ThenInclude(m => m.MaterialType)
    .Where(co =>
        (DateTime.Today - co.CheckoutDate).Days > 
        co.Material.MaterialType.CheckoutDays &&
        co.ReturnDate == null)
    .ToList();
});

app.MapGet("/checkouts", (LoncotesLibraryDbContext db) => {
    return db.Checkouts.Include(p => p.Patron)
    .Include(co => co.Material)
    .ThenInclude(m => m.MaterialType)
    .Where(co => co.ReturnDate == null ).ToList();
});

app.Run();
