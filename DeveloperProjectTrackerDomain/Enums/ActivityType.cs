

namespace DeveloperProjectTrackerDomain.Enums
{
    public enum ActivityType
    {
        ProjectCreated,
        ProjectUpdated,

        MemberAdded,
        MemberRemoved,
        MemberRoleChanged,

        TaskCreated,
        TaskUpdated,
        TaskAssigned,
        TaskStatusChanged,
        TaskPriorityChanged,
        TaskCompleted,

        CommentAdded,
        CommentDeleted,

        MilestoneCreated,
        MilestoneUpdated,
        MilestoneCompleted,

        WorkLogStarted,
        WorkLogStopped,

        LabelAdded,
        LabelRemoved
    }
}
