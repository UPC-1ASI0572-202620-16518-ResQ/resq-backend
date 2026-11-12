namespace ResQ.API.Building_Management.Domain.Model.Aggregates;

public class FloorPlanLayout
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid BuildingId { get; private set; }
    public string FloorLabel { get; private set; } = string.Empty;
    public string LayoutJson { get; private set; } = "{}";
    public int UpdatedByUserId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public long Version { get; private set; }

    protected FloorPlanLayout() { }

    public static FloorPlanLayout Create(
        Guid organizationId, Guid buildingId, string floorLabel, string layoutJson, int userId)
    {
        var layout = new FloorPlanLayout
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, BuildingId = buildingId, Version = 0
        };
        layout.Update(floorLabel, layoutJson, userId);
        return layout;
    }

    public void Update(string floorLabel, string layoutJson, int userId)
    {
        if (string.IsNullOrWhiteSpace(floorLabel)) throw new ArgumentException("Floor label is required.");
        if (string.IsNullOrWhiteSpace(layoutJson)) throw new ArgumentException("Layout is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        FloorLabel = floorLabel.Trim();
        LayoutJson = layoutJson;
        UpdatedByUserId = userId;
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }
}
