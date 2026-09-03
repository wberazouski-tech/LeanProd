using LeanProd.Application.Features.MasterData;

namespace LeanProd.Application.Features.Workforce;

public interface IBrigadeService
{
    Task<MasterDataPage<BrigadeSummary>> GetBrigadesAsync(WorkforceQuery query, CancellationToken ct);
    Task<BrigadeDetails?> GetBrigadeAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<BrigadeOption>> GetBrigadeOptionsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<BrigadeMembershipDetails>> GetMembershipsAsync(Guid brigadeId, bool history,
        CancellationToken ct);
    Task<WorkforceResult<BrigadeDetails>> CreateBrigadeAsync(SaveBrigadeCommand command, CancellationToken ct);
    Task<WorkforceResult<BrigadeDetails>> UpdateBrigadeAsync(Guid id, SaveBrigadeCommand command, CancellationToken ct);
    Task<WorkforceResult<bool>> SetBrigadeActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<WorkforceResult<BrigadeMembershipDetails>> AddMembershipAsync(Guid brigadeId,
        AddBrigadeMembershipCommand command, CancellationToken ct);
    Task<WorkforceResult<BrigadeMembershipDetails>> CloseMembershipAsync(Guid brigadeId, Guid membershipId,
        CloseBrigadeMembershipCommand command, CancellationToken ct);
    Task<WorkforceResult<BrigadeMembershipDetails>> ChangeCoefficientAsync(Guid brigadeId, Guid membershipId,
        ChangeMembershipCoefficientCommand command, CancellationToken ct);
}
