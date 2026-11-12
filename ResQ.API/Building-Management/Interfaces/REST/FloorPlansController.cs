using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Queries;
using ResQ.API.Building_Management.Domain.Services;
using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.API.Building_Management.Interfaces.REST;

[ApiController]
[Route("api/v1/buildings/{buildingId:guid}/floors/{floorLabel}/plan")]
[Authorize]
public class FloorPlansController(AppDbContext context, IBuildingQueryService buildingQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid buildingId, string floorLabel)
    {
        if (!TryContext(out var organizationId, out _)) return Unauthorized();
        var plan = await context.Set<FloorPlanLayout>().FirstOrDefaultAsync(candidate =>
            candidate.OrganizationId == organizationId && candidate.BuildingId == buildingId &&
            candidate.FloorLabel == floorLabel);
        if (plan is null) return NotFound();
        Response.Headers.ETag = $"\"{plan.Version}\"";
        return Ok(ToResource(plan));
    }

    [HttpPut]
    public async Task<IActionResult> Put(Guid buildingId, string floorLabel, [FromBody] FloorPlanResource resource)
    {
        if (!TryContext(out var organizationId, out var userId)) return Unauthorized();
        var building = await buildingQueryService.Handle(new GetBuildingByIdQuery(organizationId, buildingId));
        if (building is null) return NotFound();
        var floor = building.Floors.Select(candidate => candidate.Label).FirstOrDefault(label =>
            string.Equals(label, floorLabel, StringComparison.OrdinalIgnoreCase));
        if (floor is null) return BadRequest(new { message = "The floor does not belong to this building." });

        var json = JsonSerializer.Serialize(resource.Layout);
        var plan = await context.Set<FloorPlanLayout>().FirstOrDefaultAsync(candidate =>
            candidate.OrganizationId == organizationId && candidate.BuildingId == buildingId &&
            candidate.FloorLabel == floor);
        if (plan is null)
        {
            if (Request.Headers.ContainsKey("If-Match"))
                return Conflict(new { message = "The remote floor plan no longer exists. Reload before saving." });
            plan = FloorPlanLayout.Create(organizationId, buildingId, floor, json, userId);
            await context.Set<FloorPlanLayout>().AddAsync(plan);
        }
        else
        {
            var ifMatch = Request.Headers.IfMatch.ToString().Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(ifMatch) && ifMatch != "*" &&
                (!long.TryParse(ifMatch, out var expectedVersion) || expectedVersion != plan.Version))
                return Conflict(new { message = "The floor plan changed in another session. Reload before saving." });
            plan.Update(floor, json, userId);
        }
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "The floor plan changed while it was being saved. Reload before retrying." });
        }
        Response.Headers.ETag = $"\"{plan.Version}\"";
        return Ok(ToResource(plan));
    }

    private bool TryContext(out Guid organizationId, out int userId)
    {
        organizationId = HttpContext.Items["OrganizationId"] is Guid id ? id : Guid.Empty;
        userId = HttpContext.Items["User"] is User user ? user.Id : 0;
        return organizationId != Guid.Empty && userId > 0;
    }

    private static object ToResource(FloorPlanLayout plan)
    {
        using var document = JsonDocument.Parse(plan.LayoutJson);
        return new
        {
            plan.Id, plan.BuildingId, plan.FloorLabel, layout = document.RootElement.Clone(),
            plan.UpdatedByUserId, plan.UpdatedAt, plan.Version
        };
    }
}

public record FloorPlanResource(JsonElement Layout);
