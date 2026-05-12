namespace PpmBackend.Models.Planning.Orders
{
    public enum OperationStatus
    {
        Pending = 0,
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Blocked = 4
    }
}
