using TaskTrackerMinimalAPI.Models;
using TaskTrackerMinimalAPI.DTOs;
using TaskTrackerMinimalAPI.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace TaskTrackerMinimalAPI.Extensions
{
    public static class ProjectEndpoints
    {
        public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/projects").WithTags("Projects");


            group.MapGet("/", async (TaskTrackerDbContext db, IMapper mapper) =>
            {
                var projects = await db.Projects.Include(p => p.Tasks).ToListAsync();
                return Results.Ok(mapper.Map<IEnumerable<ProjectReadDto>>(projects));
            }).WithSummary("Get all Projects")
            .WithDescription("Returns a list of all projects with their associated Task.");

            group.MapGet("/{id}", (int id, TaskTrackerDbContext db, IMapper mapper) =>
            {
                var projects = db.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == id);
                if (projects == null) return Results.NotFound();
                return Results.Ok(mapper.Map<ProjectReadDto>(projects));
            }).WithSummary("Get Project")
            .WithDescription("Returns a projects with their associated Task.");

            group.MapPost("/", async (ProjectCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
            {
                var project = mapper.Map<Project>(dto);
                db.Projects.Add(project);
                await db.SaveChangesAsync();

                var readDto = mapper.Map<ProjectReadDto>(project);
                return Results.Created($"/api/projects/{project.Id}", readDto);
            }).WithSummary("Create Project")
            .WithDescription("Creates a project");

            group.MapPut("/{id}", async (int id, ProjectCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
            {
                var project = await db.Projects.FindAsync(id);
                if (project == null) return Results.NotFound();

                mapper.Map(dto, project);
                await db.SaveChangesAsync();
                return Results.Ok(mapper.Map<ProjectReadDto>(project));
            }).WithSummary("Update Project")
            .WithDescription("Updates a project");

            group.MapDelete("/{id}", async (int id, TaskTrackerDbContext db) =>
            {
                var project = await db.Projects.FindAsync(id);
                if (project == null) return Results.NotFound();

                db.Projects.Remove(project);
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Project deleted successfully." });
            }).WithSummary("Delete Project")
            .WithDescription("Deletes a project");

            return routes;
        }
    }
}
