namespace API.Hubs;

public record EntityChangeNotification(string EntityType, string ChangeType, Guid? Id, int Count = 1);
