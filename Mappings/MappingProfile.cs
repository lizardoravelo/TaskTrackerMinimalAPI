using AutoMapper;
using TaskTrackerMinimalAPI.DTOs;
using TaskTrackerMinimalAPI.Models;
using Task = TaskTrackerMinimalAPI.Models.Task;

namespace TaskTrackerMinimalAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectReadDto>();
            CreateMap<ProjectCreateDto, Project>();
            CreateMap<Task, TaskReadDto>();
            CreateMap<TaskCreateDto, Task>();
        }
    }
}
