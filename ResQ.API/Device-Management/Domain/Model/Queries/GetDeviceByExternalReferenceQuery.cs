namespace ResQ.API.Device_Management.Domain.Model.Queries;

public record GetDeviceByExternalReferenceQuery(Guid OrganizationId, string SourceSystem, string ExternalDeviceId);