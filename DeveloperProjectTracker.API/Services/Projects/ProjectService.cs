using DeveloperProjectTracker.API.Data;

namespace DeveloperProjectTracker.API.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }


    }
}
