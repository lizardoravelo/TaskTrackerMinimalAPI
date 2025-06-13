using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskTrackerMinimalAPI.Data;
using TaskTrackerMinimalAPI.DTOs;

namespace TaskTrackerMinimalAPI.Extensions
{
    public static class TaskEndpoints
    {
        public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder routes) 
        {

            routes.MapGet("/api/projects/{projectId}/tasks", async (int projectId, TaskTrackerDbContext db, IMapper mapper) =>
            {
                var project = await db.Projects.Include(p => p.Tasks)
                                               .FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null) return Results.NotFound();

                var tasks = mapper.Map<IEnumerable<TaskReadDto>>(project.Tasks);
                return Results.Ok(tasks);
            }).WithTags("Task")
            .WithSummary("Get all Tasks of a Project")
            .WithDescription("Return a list of tasks for a project");

            routes.MapPost("/api/projects/{projectId}/tasks", async (int projectId, TaskCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
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

            var group = routes.MapGroup("/api/tasks").WithTags("Task");

            group.MapPut("/{id}", async (int id, TaskCreateDto dto, TaskTrackerDbContext db, IMapper mapper) =>
            {
                var task = await db.Tasks.FindAsync(id);
                if (task == null) return Results.NotFound();

                mapper.Map(dto, task);
                await db.SaveChangesAsync();
                return Results.Ok(mapper.Map<TaskReadDto>(task));
            }).WithSummary("Update a Task")
            .WithDescription("Updates a Task");

            group.MapDelete("/{id}", async (int id, TaskTrackerDbContext db) =>
            {
                var task = await db.Tasks.FindAsync(id);
                if (task == null) return Results.NotFound();

                db.Tasks.Remove(task);
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Task deleted successfully." });
            }).WithSummary("Delete a Task")
            .WithDescription("Deletes a Task");
            return routes;
        }
    }
}
