using DeveloperProjectTracker.API.DTOs.ActivityLogs;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/tenants/{tenantId}/activities")]
    [Authorize]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogController(
            IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActivityLogResponseDto>>>
            GetTenantActivities(Guid tenantId)
        {
            var result =
                await _activityLogService
                    .GetTenantActivitiesAsync(tenantId);

            return Ok(result);
        }

        [HttpGet("projects/{projectId}")]
        public async Task<ActionResult<List<ActivityLogResponseDto>>>
            GetProjectActivities(
                Guid tenantId,
                Guid projectId)
        {
            var result =
                await _activityLogService
                    .GetProjectActivitiesAsync(
                        tenantId,
                        projectId);

            return Ok(result);
        }

        [HttpGet("projects/{projectId}/tasks/{taskId}")]
        public async Task<ActionResult<List<ActivityLogResponseDto>>>
            GetTaskActivities(
                Guid tenantId,
                Guid projectId,
                Guid taskId)
        {
            var result =
                await _activityLogService
                    .GetTaskActivitiesAsync(
                        tenantId,
                        projectId,
                        taskId);

            return Ok(result);
        }
    }
}
