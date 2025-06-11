using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskTrackerMinimalAPI.Models;
using Task = TaskTrackerMinimalAPI.Models.Task;

namespace TaskTrackerMinimalAPI.Data;

public class TaskTrackerDbContext : DbContext
{
    public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options) : base(options) { }
    
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<Project> Projects => Set<Project>();

}
