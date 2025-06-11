using Microsoft.EntityFrameworkCore;
using TaskTrackerMinimalAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using AutoMapper;
using TaskTrackerMinimalAPI.DTOs;

var builder = WebApplication.CreateBuilder(args);
var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0Audience = builder.Configuration["Auth0:Audience"];

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<TaskTrackerDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = auth0Domain;
        options.Audience = auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth0Domain,
            ValidateAudience = true,
            ValidAudience = auth0Audience,
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "TaskTrackerMinimalAPI", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {your JWT token}",
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:5001")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("TokenBucket", context =>
       RateLimitPartition.Get<string>(
          context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
          ip => new TokenBucketRateLimiter(
              new TokenBucketRateLimiterOptions
              {
                  TokenLimit = 10,
                  TokensPerPeriod = 5,
                  ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                  QueueLimit = 2,
                  QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                  AutoReplenishment = true,
              })
          ));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("{\"error\": \"Too many requests. Please wait a moment and try again. \"}",
            token);
    };

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ---------------------------
//          Project
// ---------------------------

app.MapGet("api/projects", async (TaskTrackerDbContext db, IMapper mapper) =>
{
    var projects = await db.Projects.Include(p => p.Tasks).ToListAsync();
    return Results.Ok(mapper.Map<IEnumerable<ProjectReadDto>>(projects));
}).WithTags("Projects")
.WithSummary("Get all Projects")
.WithDescription("Returns a list of all projects with their associated Task.");

app.MapGet("api/projects/{id}", (int id, TaskTrackerDbContext db, IMapper mapper) =>
{
    var projects = db.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == id);
    if (projects == null) return Results.NotFound();
    return Results.Ok(mapper.Map<ProjectReadDto>(projects));
}).WithTags("Projects")
.WithSummary("Get Project")
.WithDescription("Returns a projects with their associated Task.");

app.MapPost("api/projects", async (ProjectCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
{
    var project = mapper.Map<TaskTrackerMinimalAPI.Models.Project>(dto);
    db.Projects.Add(project);
    await db.SaveChangesAsync();

    var readDto = mapper.Map<ProjectReadDto>(project);
    return Results.Created($"/api/projects/{project.Id}", readDto);
}).WithTags("Projects")
.WithSummary("Create Project")
.WithDescription("Creates a project");

app.MapPut("/api/projects/{id}", async (int id, ProjectCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
{
    var project = await db.Projects.FindAsync(id);
    if (project == null) return Results.NotFound();

    mapper.Map(dto, project);
    await db.SaveChangesAsync();
    return Results.Ok(mapper.Map<ProjectReadDto>(project));
}).WithTags("Projects")
.WithSummary("Update Project")
.WithDescription("Updates a project");

app.MapDelete("/api/projects/{id}", async (int id, TaskTrackerDbContext db) =>
{
    var project = await db.Projects.FindAsync(id);
    if (project == null) return Results.NotFound();

    db.Projects.Remove(project);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Project deleted successfully." });
}).WithTags("Projects")
.WithSummary("Delete Project")
.WithDescription("Deletes a project");

// ------------------------
//         TASKS
// ------------------------

app.MapGet("/api/projects/{projectId}/tasks", async (int projectId, TaskTrackerDbContext db, IMapper mapper) =>
{
    var project = await db.Projects.Include(p => p.Tasks)
                                   .FirstOrDefaultAsync(p => p.Id == projectId);
    if (project == null) return Results.NotFound();

    var tasks = mapper.Map<IEnumerable<TaskReadDto>>(project.Tasks);
    return Results.Ok(tasks);
}).WithTags("Task")
.WithSummary("Get all Tasks of a Project")
.WithDescription("Return a list of tasks for a project");

app.MapPost("/api/projects/{projectId}/tasks", async (int projectId, TaskCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
{
    var project = await db.Projects.FindAsync(projectId);
    if (project == null) return Results.NotFound();

    var task = mapper.Map<TaskTrackerMinimalAPI.Models.Task>(dto);
    task.ProjectId = projectId;

    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tasks/{task.Id}", mapper.Map<TaskReadDto>(task));
}).WithTags("Task")
.WithSummary("Create a Task")
.WithDescription("Creates a Task inside a project");

app.MapPut("/api/tasks/{id}", async (int id, TaskCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    mapper.Map(dto, task);
    await db.SaveChangesAsync();
    return Results.Ok(mapper.Map<TaskReadDto>(task));
}).WithTags("Task")
.WithSummary("Update a Task")
.WithDescription("Updates a Task");

app.MapDelete("/api/tasks/{id}", async (int id, TaskTrackerDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Task deleted successfully." });
}).WithTags("Task")
.WithSummary("Delete a Task")
.WithDescription("Deletes a Task");

app.Run();