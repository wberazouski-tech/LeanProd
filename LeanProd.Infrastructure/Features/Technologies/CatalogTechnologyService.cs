using System.Globalization;
using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Technologies;
using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.Technologies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Technologies;

public sealed class CatalogTechnologyService(LeanProdDbContext db) : ICatalogTechnologyService
{
    private const int GeneratedCodeLength = 4;

    public Task<bool> HasTechnologyStageDuplicateAsync(string name, Guid departmentId, CancellationToken ct)
    {
        var normalizedName = name.Trim().ToUpperInvariant();
        return db.TechnologyStages.AsNoTracking().AnyAsync(stage =>
            stage.DepartmentId == departmentId && stage.Name.ToUpper() == normalizedName, ct);
    }

    public async Task<MasterDataPage<CatalogTechnologySummary>> GetTechnologiesAsync(
        CatalogTechnologyQuery query, CancellationToken ct)
    {
        var source = db.CatalogTechnologies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Code.Contains(search) || x.Name.Contains(search)
                || (x.CatalogItem != null && x.CatalogItem.WorkingName.Contains(search))
                || (x.CatalogItemClass != null && x.CatalogItemClass.Name.Contains(search)));
        }

        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (query.CatalogItemId is not null) source = source.Where(x => x.CatalogItemId == query.CatalogItemId);
        if (query.CatalogItemClassId is not null) source = source.Where(x => x.CatalogItemClassId == query.CatalogItemClassId);

        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Code)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new CatalogTechnologySummary(
                x.Id, x.Code, x.Name,
                x.CatalogItemId, x.CatalogItem == null ? null : x.CatalogItem.WorkingName,
                x.CatalogItemClassId, x.CatalogItemClass == null ? null : x.CatalogItemClass.Code,
                x.CatalogItemClass == null ? null : x.CatalogItemClass.Name,
                x.VersionNo, x.ValidFrom, x.ValidTo, x.IsDefault, x.Status, x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<CatalogTechnologyDetails?> GetTechnologyAsync(Guid id, CancellationToken ct)
    {
        var item = await QueryDetails().SingleOrDefaultAsync(x => x.Id == id, ct);
        return item is null ? null : Details(item);
    }

    public async Task<MasterDataResult<CatalogTechnologyDetails>> CreateTechnologyAsync(
        SaveCatalogTechnologyCommand command, CancellationToken ct)
    {
        command = await EnsureGeneratedCodesAsync(command, ct);
        command = await ResolveTechnologyStagesAsync(command, ct);
        command = command with { Status = CatalogTechnologyStatus.InDevelopment };
        var validation = await Validate(command, null, ct);
        if (validation is not null) return validation;

        var technology = new CatalogTechnology();
        ApplyHeader(technology, command);
        ApplyChildren(technology, command);
        db.CatalogTechnologies.Add(technology);

        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<CatalogTechnologyDetails>.Success((await GetTechnologyAsync(technology.Id, ct))!);
        }
        catch (DbUpdateException)
        {
            return Conflict("A technology with this code already exists.");
        }
    }

    public async Task<MasterDataResult<CatalogTechnologyDetails>> UpdateTechnologyAsync(
        Guid id, SaveCatalogTechnologyCommand command, CancellationToken ct)
    {
        var technology = await db.CatalogTechnologies
            .Include(x => x.StageTransitions)
            .Include(x => x.Stages).ThenInclude(x => x.Materials).ThenInclude(x => x.RouteSteps)
            .Include(x => x.Stages).ThenInclude(x => x.Outputs)
            .Include(x => x.Stages).ThenInclude(x => x.Operations)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (technology is null)
            return MasterDataResult<CatalogTechnologyDetails>.Failure(MasterDataError.NotFound, "Record was not found.");

        command = await EnsureGeneratedCodesAsync(command, ct);
        command = await ResolveTechnologyStagesAsync(command, ct);
        var validation = await Validate(command, id, ct);
        if (validation is not null) return validation;
        if (!SetVersion(technology, command.RowVersion)) return Validation("Row version is required.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.CatalogTechnologyStageTransitions.RemoveRange(technology.StageTransitions);
        db.CatalogTechnologyMaterialSupplyRouteSteps.RemoveRange(technology.Stages.SelectMany(x => x.Materials).SelectMany(x => x.RouteSteps));
        db.CatalogTechnologyMaterials.RemoveRange(technology.Stages.SelectMany(x => x.Materials));
        db.CatalogTechnologyStageOutputs.RemoveRange(technology.Stages.SelectMany(x => x.Outputs));
        db.CatalogTechnologyOperations.RemoveRange(technology.Stages.SelectMany(x => x.Operations));
        db.CatalogTechnologyStages.RemoveRange(technology.Stages);

        ApplyHeader(technology, command);

        try
        {
            await db.SaveChangesAsync(ct);
            technology.StageTransitions = [];
            technology.Stages = [];
            ApplyChildren(technology, command);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return MasterDataResult<CatalogTechnologyDetails>.Success((await GetTechnologyAsync(technology.Id, ct))!);
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync(ct);
            return Conflict("The technology was changed by another request.");
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(ct);
            return Conflict("A technology with this code already exists or contains duplicate rows.");
        }
    }

    public async Task<MasterDataResult<bool>> SetTechnologyActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var technology = await db.CatalogTechnologies.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (technology is null)
            return MasterDataResult<bool>.Failure(MasterDataError.NotFound, "Record was not found.");
        technology.IsActive = active;
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<bool>.Success(true);
        }
        catch (DbUpdateException)
        {
            return MasterDataResult<bool>.Failure(MasterDataError.Conflict,
                "Only one active default technology is allowed for one item or class.");
        }
    }

    public async Task<IReadOnlyCollection<TechnologyStageTemplateDetails>> GetStageTemplatesAsync(
        bool activeOnly, CancellationToken ct)
    {
        var source = db.TechnologyStageTemplates.AsNoTracking();
        if (activeOnly) source = source.Where(x => x.IsActive);
        return await source.OrderBy(x => x.Code)
            .Select(x => new TechnologyStageTemplateDetails(
                x.Id, x.Code, x.Name, x.Description, x.IsActive, Convert.ToBase64String(x.RowVersion)))
            .ToArrayAsync(ct);
    }

    public async Task<IReadOnlyCollection<TechnologyStageDetails>> GetTechnologyStagesAsync(
        bool activeOnly, CancellationToken ct)
    {
        var source = db.TechnologyStages.AsNoTracking();
        if (activeOnly) source = source.Where(x => x.IsActive);
        return await source.OrderBy(x => x.Code).Select(x => new TechnologyStageDetails(
            x.Id, x.Code, x.Name, x.Description, x.IsActive, x.DepartmentId,
            x.Department == null ? null : x.Department.Name)).ToArrayAsync(ct);
    }

    public async Task<MasterDataResult<TechnologyStageTemplateDetails>> SaveStageTemplateAsync(
        Guid? id, SaveTechnologyStageTemplateCommand command, CancellationToken ct)
    {
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.TechnologyStageTemplates.AsNoTracking().Select(x => x.Code), GeneratedCodeLength, ct)
        };
        if (string.IsNullOrWhiteSpace(command.Name)) return TemplateValidation("Name is required.");
        if (command.Code.Trim().Length > 50) return TemplateValidation("Code cannot exceed 50 characters.");
        if (command.Name.Trim().Length > 200) return TemplateValidation("Name cannot exceed 200 characters.");

        var template = id is null
            ? new TechnologyStageTemplate()
            : await db.TechnologyStageTemplates.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (template is null)
            return MasterDataResult<TechnologyStageTemplateDetails>.Failure(MasterDataError.NotFound, "Record was not found.");
        if (id is not null && !SetVersion(template, command.RowVersion)) return TemplateValidation("Row version is required.");

        template.Code = command.Code.Trim().ToUpperInvariant();
        template.Name = command.Name.Trim();
        template.Description = Clean(command.Description);
        template.IsActive = command.IsActive;
        if (id is null) db.TechnologyStageTemplates.Add(template);

        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<TechnologyStageTemplateDetails>.Success(new(
                template.Id, template.Code, template.Name, template.Description,
                template.IsActive, Convert.ToBase64String(template.RowVersion)));
        }
        catch (DbUpdateConcurrencyException)
        {
            return TemplateConflict("The stage template was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return TemplateConflict("A stage template with this code already exists.");
        }
    }

    private async Task<SaveCatalogTechnologyCommand> EnsureGeneratedCodesAsync(
        SaveCatalogTechnologyCommand command, CancellationToken ct)
    {
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.CatalogTechnologies.AsNoTracking().Select(x => x.Code), GeneratedCodeLength, ct)
        };

        var existingStageCodes = (await db.TechnologyStages.AsNoTracking()
            .Select(x => x.Code).ToArrayAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingOperationCodes = (await db.CatalogTechnologyOperations.AsNoTracking()
            .Select(x => x.Code).ToArrayAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var nextStage = await db.TechnologyStages.CountAsync(ct) + 1;
        var nextOperation = await db.CatalogTechnologyOperations.CountAsync(ct) + 1;
        foreach (var stage in command.Stages)
        {
            if (!string.IsNullOrWhiteSpace(stage.TechnologyStageCode))
                existingStageCodes.Add(stage.TechnologyStageCode.Trim().ToUpperInvariant());
            foreach (var operation in stage.Operations.Where(x => !string.IsNullOrWhiteSpace(x.Code)))
                existingOperationCodes.Add(operation.Code.Trim().ToUpperInvariant());
        }

        var stages = command.Stages.Select(stage =>
        {
            var operations = stage.Operations.Select(operation =>
                string.IsNullOrWhiteSpace(operation.Code)
                    ? operation with { Code = NextBatchCode(existingOperationCodes, ref nextOperation) }
                    : operation).ToArray();

            return string.IsNullOrWhiteSpace(stage.TechnologyStageCode)
                ? stage with
                {
                    TechnologyStageCode = NextBatchCode(existingStageCodes, ref nextStage),
                    Operations = operations
                }
                : stage with { Operations = operations };
        }).ToArray();

        return command with { Stages = stages };
    }

    private async Task<SaveCatalogTechnologyCommand> ResolveTechnologyStagesAsync(
        SaveCatalogTechnologyCommand command, CancellationToken ct)
    {
        var requestedCodes = command.Stages
            .Where(x => x.TechnologyStageId is null)
            .Select(x => x.TechnologyStageCode.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var known = await db.TechnologyStages.Where(x => requestedCodes.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var input in command.Stages.Where(x => x.TechnologyStageId is null))
        {
            var code = input.TechnologyStageCode.Trim().ToUpperInvariant();
            if (known.ContainsKey(code)) continue;
            var stage = new TechnologyStage
            {
                Code = code,
                Name = input.TechnologyStageName.Trim(),
                DepartmentId = input.TechnologyStageDepartmentId
            };
            db.TechnologyStages.Add(stage);
            known.Add(code, stage);
        }

        return command with
        {
            Stages = command.Stages.Select(x => x.TechnologyStageId is null
                ? x with { TechnologyStageId = known[x.TechnologyStageCode.Trim().ToUpperInvariant()].Id }
                : x).ToArray()
        };
    }

    private static string NextBatchCode(HashSet<string> existingCodes, ref int next)
    {
        while (true)
        {
            var candidate = next.ToString(CultureInfo.InvariantCulture).PadLeft(GeneratedCodeLength, '0');
            next++;
            if (!existingCodes.Add(candidate)) continue;
            return candidate;
        }
    }

    private IQueryable<CatalogTechnology> QueryDetails() => db.CatalogTechnologies.AsNoTracking()
        .Include(x => x.CatalogItem)
        .Include(x => x.CatalogItemClass)
        .Include(x => x.StageTransitions)
        .Include(x => x.Stages).ThenInclude(x => x.TechnologyStage).ThenInclude(x => x.Department)
        .Include(x => x.Stages).ThenInclude(x => x.Equipment)
        .Include(x => x.Stages).ThenInclude(x => x.Materials).ThenInclude(x => x.CatalogItem)
        .Include(x => x.Stages).ThenInclude(x => x.Materials).ThenInclude(x => x.UnitOfMeasure)
        .Include(x => x.Stages).ThenInclude(x => x.Materials).ThenInclude(x => x.DefaultSourceStorageLocation)
        .Include(x => x.Stages).ThenInclude(x => x.Materials).ThenInclude(x => x.RouteSteps).ThenInclude(x => x.FromStorageLocation)
        .Include(x => x.Stages).ThenInclude(x => x.Materials).ThenInclude(x => x.RouteSteps).ThenInclude(x => x.ToStorageLocation)
        .Include(x => x.Stages).ThenInclude(x => x.Outputs).ThenInclude(x => x.CatalogItem)
        .Include(x => x.Stages).ThenInclude(x => x.Outputs).ThenInclude(x => x.UnitOfMeasure)
        .Include(x => x.Stages).ThenInclude(x => x.Outputs).ThenInclude(x => x.ReceiptStorageLocation)
        .Include(x => x.Stages).ThenInclude(x => x.Operations).ThenInclude(x => x.Department)
        .Include(x => x.Stages).ThenInclude(x => x.Operations).ThenInclude(x => x.Equipment)
        .AsSplitQuery();

    private async Task<MasterDataResult<CatalogTechnologyDetails>?> Validate(
        SaveCatalogTechnologyCommand command, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Code)) return Validation("Code is required.");
        if (string.IsNullOrWhiteSpace(command.Name)) return Validation("Name is required.");
        if (command.Code.Trim().Length > 50) return Validation("Code cannot exceed 50 characters.");
        if (command.Name.Trim().Length > 200) return Validation("Name cannot exceed 200 characters.");
        if (command.VersionNo < 1) return Validation("Version number must be positive.");
        if (!Enum.IsDefined(command.Status)) return Validation("Technology status is invalid.");
        if (command.ValidFrom is not null && command.ValidTo is not null && command.ValidTo < command.ValidFrom)
            return Validation("Valid to cannot be earlier than valid from.");
        if ((command.CatalogItemId is null) == (command.CatalogItemClassId is null))
            return Validation("Technology must target either one catalog item or one catalog item class.");
        if (command.Status != CatalogTechnologyStatus.InDevelopment && command.Stages.Count == 0)
            return Validation("At least one technology stage is required outside In development status.");

        if (await db.CatalogTechnologies.AnyAsync(x => x.Id != id && x.Code == command.Code.Trim().ToUpperInvariant(), ct))
            return Conflict("A technology with this code already exists.");
        if (command.IsDefault && command.CatalogItemId is not null
            && await db.CatalogTechnologies.AnyAsync(x => x.Id != id && x.CatalogItemId == command.CatalogItemId && x.IsDefault && x.IsActive, ct))
            return Conflict("Only one active default technology is allowed for one item.");
        if (command.IsDefault && command.CatalogItemClassId is not null
            && await db.CatalogTechnologies.AnyAsync(x => x.Id != id && x.CatalogItemClassId == command.CatalogItemClassId && x.IsDefault && x.IsActive, ct))
            return Conflict("Only one active default technology is allowed for one class.");

        if (command.CatalogItemId is not null
            && !await db.CatalogItems.AnyAsync(x => x.Id == command.CatalogItemId && x.Type == CatalogItemType.Product && x.IsActive, ct))
            return Validation("Technology item target must be an active product.");
        if (command.CatalogItemClassId is not null
            && !await db.CatalogItemClasses.AnyAsync(x => x.Id == command.CatalogItemClassId && x.Type == CatalogItemType.Product && !x.IsGroup && x.IsActive, ct))
            return Validation("Technology class target must be an active product class.");

        var stageIds = new HashSet<Guid>();
        var stageNumbers = new HashSet<int>();
        foreach (var stage in command.Stages)
        {
            if (stage.Id is null || stage.Id == Guid.Empty) return Validation("Each stage must have an id.");
            if (!stageIds.Add(stage.Id.Value)) return Validation("Stage ids must be unique.");
            if (!stageNumbers.Add(stage.StageNumber)) return Validation("Stage numbers must be unique.");
            if (string.IsNullOrWhiteSpace(stage.TechnologyStageCode) || string.IsNullOrWhiteSpace(stage.TechnologyStageName))
                return Validation("Stage code and name are required.");
            if (stage.StageNumber < 1) return Validation("Stage number must be positive.");
            if (stage.PlannedDurationMinutes < 0) return Validation("Stage duration cannot be negative.");
            if (stage.TechnologyStageId is not null
                && !db.TechnologyStages.Local.Any(x => x.Id == stage.TechnologyStageId)
                && !await db.TechnologyStages.AnyAsync(x => x.Id == stage.TechnologyStageId && x.IsActive, ct))
                return Validation("Technology stage must be active.");
            if (stage.TechnologyStageDepartmentId is null
                || !await db.Departments.AnyAsync(x => x.Id == stage.TechnologyStageDepartmentId && x.IsActive, ct))
                return Validation("Stage department must be active.");
            if (stage.EquipmentId is not null
                && !await db.Equipment.AnyAsync(x => x.Id == stage.EquipmentId && x.IsActive, ct))
                return Validation("Stage equipment must be active.");

            foreach (var material in stage.Materials)
            {
                if (material.Quantity <= 0) return Validation("Material quantity must be positive.");
                if (material.ScrapPercent is < 0 or > 100) return Validation("Material scrap percent must be between 0 and 100.");
                if (!Enum.IsDefined(material.ConsumptionTrackingMode)) return Validation("Material consumption tracking mode is invalid.");
                if (!await db.CatalogItems.AnyAsync(x => x.Id == material.CatalogItemId && x.IsActive, ct))
                    return Validation("Material item must be active.");
                if (!await db.UnitOfMeasures.AnyAsync(x => x.Id == material.UnitOfMeasureId && x.IsActive, ct))
                    return Validation("Material unit must be active.");
                if (material.DefaultSourceStorageLocationId is not null
                    && !await db.StorageLocations.AnyAsync(x => x.Id == material.DefaultSourceStorageLocationId && x.IsActive, ct))
                    return Validation("Material source storage location must be active.");
                var routeLines = new HashSet<int>();
                foreach (var routeStep in material.RouteSteps)
                {
                    if (routeStep.LineNo < 1 || !routeLines.Add(routeStep.LineNo))
                        return Validation("Route step line numbers must be positive and unique inside a material.");
                    if ((routeStep.ToStorageLocationId is null) != routeStep.IsConsumptionPoint)
                        return Validation("Route step must have either target storage or consumption point.");
                    if (routeStep.LeadTimeMinutes < 0) return Validation("Route lead time cannot be negative.");
                    if (!Enum.IsDefined(routeStep.MovementKind)) return Validation("Route movement kind is invalid.");
                    if (!await db.StorageLocations.AnyAsync(x => x.Id == routeStep.FromStorageLocationId && x.IsActive, ct))
                        return Validation("Route source storage location must be active.");
                    if (routeStep.ToStorageLocationId is not null
                        && !await db.StorageLocations.AnyAsync(x => x.Id == routeStep.ToStorageLocationId && x.IsActive, ct))
                        return Validation("Route target storage location must be active.");
                }
            }

            foreach (var output in stage.Outputs)
            {
                if (output.Quantity <= 0) return Validation("Output quantity must be positive.");
                if (!await db.CatalogItems.AnyAsync(x => x.Id == output.CatalogItemId && x.IsActive, ct))
                    return Validation("Output item must be active.");
                if (!await db.UnitOfMeasures.AnyAsync(x => x.Id == output.UnitOfMeasureId && x.IsActive, ct))
                    return Validation("Output unit must be active.");
                if (!await db.StorageLocations.AnyAsync(x => x.Id == output.ReceiptStorageLocationId && x.IsActive, ct))
                    return Validation("Output receipt storage location must be active.");
            }

            foreach (var operation in stage.Operations)
            {
                if (string.IsNullOrWhiteSpace(operation.Code) || string.IsNullOrWhiteSpace(operation.Name))
                    return Validation("Operation code and name are required.");
                if (operation.SetupMinutes < 0 || operation.RunMinutes < 0 || operation.LaborMinutes < 0)
                    return Validation("Operation minutes cannot be negative.");
                if (operation.Workers <= 0) return Validation("Operation workers must be positive.");
                if (operation.DepartmentId is not null
                    && !await db.Departments.AnyAsync(x => x.Id == operation.DepartmentId && x.IsActive, ct))
                    return Validation("Operation department must be active.");
                if (operation.EquipmentId is not null
                    && !await db.Equipment.AnyAsync(x => x.Id == operation.EquipmentId && x.IsActive, ct))
                    return Validation("Operation equipment must be active.");
            }
        }

        var graphValidation = ValidateStageGraph(command.StageTransitions, stageIds);
        return graphValidation is null ? null : Validation(graphValidation);
    }

    private static string? ValidateStageGraph(
        IReadOnlyCollection<SaveCatalogTechnologyStageTransitionCommand> links, HashSet<Guid> stageIds)
    {
        var adjacency = new Dictionary<Guid, List<Guid>>();
        foreach (var link in links)
        {
            if (!stageIds.Contains(link.FromCatalogTechnologyStageId) || !stageIds.Contains(link.ToCatalogTechnologyStageId))
                return "Stage transitions must reference stages from the same technology.";
            if (link.FromCatalogTechnologyStageId == link.ToCatalogTechnologyStageId) return "Stage cannot transition to itself.";
            adjacency.TryAdd(link.FromCatalogTechnologyStageId, []);
            adjacency[link.FromCatalogTechnologyStageId].Add(link.ToCatalogTechnologyStageId);
        }

        var visiting = new HashSet<Guid>();
        var visited = new HashSet<Guid>();
        bool HasCycle(Guid id)
        {
            if (visited.Contains(id)) return false;
            if (!visiting.Add(id)) return true;
            foreach (var next in adjacency.GetValueOrDefault(id, []))
                if (HasCycle(next)) return true;
            visiting.Remove(id);
            visited.Add(id);
            return false;
        }

        return stageIds.Any(HasCycle) ? "Stage graph cannot contain cycles." : null;
    }

    private static void ApplyHeader(CatalogTechnology technology, SaveCatalogTechnologyCommand command)
    {
        technology.Code = command.Code.Trim().ToUpperInvariant();
        technology.Name = command.Name.Trim();
        technology.CatalogItemId = command.CatalogItemId;
        technology.CatalogItemClassId = command.CatalogItemClassId;
        technology.VersionNo = command.VersionNo;
        technology.ValidFrom = command.ValidFrom;
        technology.ValidTo = command.ValidTo;
        technology.IsDefault = command.IsDefault;
        technology.Status = command.Status;
        technology.Description = Clean(command.Description);
    }

    private static void ApplyChildren(CatalogTechnology technology, SaveCatalogTechnologyCommand command)
    {
        foreach (var input in command.Stages.OrderBy(x => x.StageNumber))
        {
            var stage = new CatalogTechnologyStage
            {
                Id = input.Id!.Value,
                CatalogTechnologyId = technology.Id,
                TechnologyStageId = input.TechnologyStageId!.Value,
                StageNumber = input.StageNumber,
                PlannedDurationMinutes = input.PlannedDurationMinutes,
                EquipmentId = input.EquipmentId,
                Description = Clean(input.Description)
            };
            stage.Materials.AddRange(input.Materials.Select(Material));
            stage.Outputs.AddRange(input.Outputs.Select(Output));
            stage.Operations.AddRange(input.Operations.Select(Operation));
            technology.Stages.Add(stage);
        }

        technology.StageTransitions.AddRange(command.StageTransitions.Select(x => new CatalogTechnologyStageTransition
        {
            Id = x.Id.GetValueOrDefault(Guid.NewGuid()),
            CatalogTechnologyId = technology.Id,
            FromCatalogTechnologyStageId = x.FromCatalogTechnologyStageId,
            ToCatalogTechnologyStageId = x.ToCatalogTechnologyStageId
        }));
    }

    private static CatalogTechnologyMaterial Material(SaveCatalogTechnologyMaterialCommand input)
    {
        var material = new CatalogTechnologyMaterial
        {
            Id = input.Id.GetValueOrDefault(Guid.NewGuid()),
            CatalogItemId = input.CatalogItemId,
            UnitOfMeasureId = input.UnitOfMeasureId,
            Quantity = input.Quantity,
            ConsumptionTrackingMode = input.ConsumptionTrackingMode,
            DefaultSourceStorageLocationId = input.DefaultSourceStorageLocationId,
            ScrapPercent = input.ScrapPercent,
            IsOptional = input.IsOptional,
            Note = Clean(input.Note)
        };
        material.RouteSteps.AddRange(input.RouteSteps.OrderBy(x => x.LineNo).Select(RouteStep));
        return material;
    }

    private static CatalogTechnologyStageOutput Output(SaveCatalogTechnologyStageOutputCommand input) => new()
    {
        Id = input.Id.GetValueOrDefault(Guid.NewGuid()),
        CatalogItemId = input.CatalogItemId,
        UnitOfMeasureId = input.UnitOfMeasureId,
        Quantity = input.Quantity,
        ReceiptStorageLocationId = input.ReceiptStorageLocationId,
        IsPrimary = input.IsPrimary,
        Note = Clean(input.Note)
    };

    private static CatalogTechnologyOperation Operation(SaveCatalogTechnologyOperationCommand input) => new()
    {
        Id = input.Id.GetValueOrDefault(Guid.NewGuid()),
        Code = input.Code.Trim().ToUpperInvariant(),
        Name = input.Name.Trim(),
        DepartmentId = input.DepartmentId,
        EquipmentId = input.EquipmentId,
        SetupMinutes = input.SetupMinutes,
        RunMinutes = input.RunMinutes,
        LaborMinutes = input.LaborMinutes,
        Workers = input.Workers,
        Note = Clean(input.Note)
    };

    private static CatalogTechnologyMaterialSupplyRouteStep RouteStep(
        SaveCatalogTechnologyMaterialSupplyRouteStepCommand input) => new()
    {
        Id = input.Id.GetValueOrDefault(Guid.NewGuid()),
        LineNo = input.LineNo,
        FromStorageLocationId = input.FromStorageLocationId,
        ToStorageLocationId = input.ToStorageLocationId,
        IsConsumptionPoint = input.IsConsumptionPoint,
        MovementKind = input.MovementKind,
        LeadTimeMinutes = input.LeadTimeMinutes,
        Note = Clean(input.Note)
    };

    private static CatalogTechnologyDetails Details(CatalogTechnology x) => new(
        x.Id, x.Code, x.Name,
        x.CatalogItemId, x.CatalogItem?.WorkingName,
        x.CatalogItemClassId, x.CatalogItemClass?.Code, x.CatalogItemClass?.Name,
        x.VersionNo, x.ValidFrom, x.ValidTo, x.IsDefault, x.Status, x.Description, x.IsActive,
        x.Stages.OrderBy(stage => stage.StageNumber).Select(Stage).ToArray(),
        x.StageTransitions.OrderBy(link => link.FromCatalogTechnologyStageId).ThenBy(link => link.ToCatalogTechnologyStageId).Select(Transition).ToArray(),
        Convert.ToBase64String(x.RowVersion));

    private static CatalogTechnologyStageDto Stage(CatalogTechnologyStage x) => new(
        x.Id, x.TechnologyStageId, x.TechnologyStage.Code, x.TechnologyStage.Name, x.StageNumber,
        x.PlannedDurationMinutes, x.TechnologyStage.DepartmentId, x.TechnologyStage.Department?.Name, x.EquipmentId,
        x.Equipment?.Name, x.Description,
        x.Materials.OrderBy(m => m.CatalogItem.WorkingName).Select(MaterialDto).ToArray(),
        x.Outputs.OrderBy(o => o.CatalogItem.WorkingName).Select(OutputDto).ToArray(),
        x.Operations.OrderBy(o => o.Code).Select(OperationDto).ToArray());

    private static CatalogTechnologyStageTransitionDto Transition(CatalogTechnologyStageTransition x) =>
        new(x.Id, x.FromCatalogTechnologyStageId, x.ToCatalogTechnologyStageId);

    private static CatalogTechnologyMaterialDto MaterialDto(CatalogTechnologyMaterial x) => new(
        x.Id, x.CatalogItemId, x.CatalogItem.WorkingName, x.UnitOfMeasureId, x.UnitOfMeasure.Name,
        x.Quantity, x.ConsumptionTrackingMode, x.DefaultSourceStorageLocationId,
        x.DefaultSourceStorageLocation?.Name, x.ScrapPercent, x.IsOptional, x.Note,
        x.RouteSteps.OrderBy(r => r.LineNo).Select(RouteStepDto).ToArray());

    private static CatalogTechnologyStageOutputDto OutputDto(CatalogTechnologyStageOutput x) => new(
        x.Id, x.CatalogItemId, x.CatalogItem.WorkingName, x.UnitOfMeasureId, x.UnitOfMeasure.Name,
        x.Quantity, x.ReceiptStorageLocationId, x.ReceiptStorageLocation.Name, x.IsPrimary, x.Note);

    private static CatalogTechnologyOperationDto OperationDto(CatalogTechnologyOperation x) => new(
        x.Id, x.Code, x.Name, x.DepartmentId, x.Department?.Name, x.EquipmentId, x.Equipment?.Name,
        x.SetupMinutes, x.RunMinutes, x.LaborMinutes, x.Workers, x.Note);

    private static CatalogTechnologyMaterialSupplyRouteStepDto RouteStepDto(
        CatalogTechnologyMaterialSupplyRouteStep x) => new(
        x.Id, x.LineNo, x.FromStorageLocationId, x.FromStorageLocation.Name,
        x.ToStorageLocationId, x.ToStorageLocation?.Name, x.IsConsumptionPoint,
        x.MovementKind, x.LeadTimeMinutes, x.Note);

    private bool SetVersion(AuditableEntity entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<CatalogTechnologyDetails> Validation(string message) =>
        MasterDataResult<CatalogTechnologyDetails>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<CatalogTechnologyDetails> Conflict(string message) =>
        MasterDataResult<CatalogTechnologyDetails>.Failure(MasterDataError.Conflict, message);
    private static MasterDataResult<TechnologyStageTemplateDetails> TemplateValidation(string message) =>
        MasterDataResult<TechnologyStageTemplateDetails>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<TechnologyStageTemplateDetails> TemplateConflict(string message) =>
        MasterDataResult<TechnologyStageTemplateDetails>.Failure(MasterDataError.Conflict, message);
}
