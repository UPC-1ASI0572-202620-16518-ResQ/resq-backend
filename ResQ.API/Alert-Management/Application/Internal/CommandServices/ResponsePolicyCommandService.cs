using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Commands;
using ResQ.API.Alert_Management.Domain.Model.ValueObjects;
using ResQ.API.Alert_Management.Domain.Repositories;
using ResQ.API.Alert_Management.Domain.Services;
using ResQ.API.Device_Management.Domain.Model.ValueObjects;
using ResQ.API.Device_Management.Interfaces.ACL;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.Alert_Management.Application.Internal.CommandServices;

public class ResponsePolicyCommandService(
    IResponsePolicyRepository responsePolicyRepository,
    IDevicesContextFacade devicesContextFacade,
    IUnitOfWork unitOfWork)
    : IResponsePolicyCommandService
{
    public async Task<ResponsePolicy?> Handle(ConfigureResponsePolicyCommand command)
    {
        var policy = ResponsePolicy.Configure(command.OrganizationId, command.RiskTypeCode, command.Actions);

        await ValidateActionTargetsAsync(command.OrganizationId, policy);

        await responsePolicyRepository.AddAsync(policy);
        await unitOfWork.CompleteAsync();

        return policy;
    }

    public async Task<ResponsePolicy?> Handle(UpdateResponsePolicyCommand command)
    {
        var policy = await FindPolicyAsync(command.OrganizationId, command.PolicyId);

        policy.Update(command.RiskTypeCode, command.Actions);

        await ValidateActionTargetsAsync(command.OrganizationId, policy);

        if (policy.IsActive)
            await EnsureNoOtherActivePolicyAsync(policy);

        await unitOfWork.CompleteAsync();

        return policy;
    }

    public async Task<ResponsePolicy?> Handle(ChangeResponsePolicyStatusCommand command)
    {
        var policy = await FindPolicyAsync(command.OrganizationId, command.PolicyId);

        if (command.Active && !policy.IsActive)
        {
            await ValidateActionTargetsAsync(command.OrganizationId, policy);
            await EnsureNoOtherActivePolicyAsync(policy);
        }

        policy.ChangeStatus(command.Active);

        await unitOfWork.CompleteAsync();

        return policy;
    }

    private async Task<ResponsePolicy> FindPolicyAsync(Guid organizationId, Guid policyId)
    {
        return await responsePolicyRepository.FindByIdAndOrganizationIdAsync(policyId, organizationId)
               ?? throw new KeyNotFoundException("Response policy not found.");
    }

    /// <summary>
    /// Only one active policy per risk type is allowed, so an alert always resolves to a single set of actions.
    /// </summary>
    private async Task EnsureNoOtherActivePolicyAsync(ResponsePolicy policy)
    {
        var active = await responsePolicyRepository.FindActiveByRiskTypeCodeAsync(policy.OrganizationId, policy.RiskTypeCode);

        if (active is not null && active.Id != policy.Id)
            throw new InvalidOperationException($"Another response policy is already active for risk type '{policy.RiskTypeCode}'.");
    }

    /// <summary>
    /// Validates each target through the Device Management ACL: the device must exist, must not be retired
    /// and must expose the target capability as an actuation capability.
    /// </summary>
    private async Task ValidateActionTargetsAsync(Guid organizationId, ResponsePolicy policy)
    {
        foreach (var action in policy.Actions)
        {
            var device = await devicesContextFacade.GetDeviceCatalogEntry(organizationId, action.TargetDeviceId);

            if (device is null)
                throw new InvalidOperationException($"Target device '{action.TargetDeviceId}' of action '{action.ActionCode}' was not found.");

            if (device.AdministrativeStatus == EDeviceAdministrativeStatus.Retired)
                throw new InvalidOperationException($"Target device '{device.DeviceCode}' of action '{action.ActionCode}' is retired.");

            var capability = device.Capabilities.FirstOrDefault(item =>
                string.Equals(item.Code, action.TargetCapabilityCode, StringComparison.OrdinalIgnoreCase));

            if (capability is null)
                throw new InvalidOperationException(
                    $"Device '{device.DeviceCode}' does not have capability '{action.TargetCapabilityCode}' required by action '{action.ActionCode}'.");

            if (capability.Kind != ECapabilityKind.Actuation)
                throw new InvalidOperationException(
                    $"Capability '{capability.Code}' of device '{device.DeviceCode}' is not an actuation capability.");
        }
    }
}
