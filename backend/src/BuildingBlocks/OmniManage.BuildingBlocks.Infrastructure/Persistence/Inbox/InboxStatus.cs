namespace OmniManage.BuildingBlocks.Infrastructure.Persistence.Inbox;

public enum InboxStatus
{
    Processed = 0,
    SkippedModuleDisabled = 1,
    Failed = 2,
    DeadLetter = 3,
}
