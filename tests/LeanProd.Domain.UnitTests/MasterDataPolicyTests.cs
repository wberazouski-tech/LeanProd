using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using Xunit;

namespace LeanProd.Domain.UnitTests;

public sealed class MasterDataPolicyTests
{
    [Theory]
    [InlineData("", "Name", "Code and name are required.")]
    [InlineData("CODE", "", "Code and name are required.")]
    [InlineData("123", "Name", "Department code must contain exactly 4 characters.")]
    [InlineData("12345", "Name", "Department code must contain exactly 4 characters.")]
    [InlineData(" 1234 ", " Department ", null)]
    public void Department_input_returns_expected_error(string code, string name, string? expected)
    {
        Assert.Equal(expected, DepartmentPolicy.ValidateInput(code, name));
    }

    [Fact]
    public void Hierarchy_rejects_self_parent()
    {
        var id = Guid.NewGuid();

        var error = HierarchyPolicy.Validate(id, id, [], "A department");

        Assert.Equal("A department cannot be its own parent.", error);
    }

    [Fact]
    public void Hierarchy_rejects_indirect_cycle()
    {
        var id = Guid.NewGuid();

        var error = HierarchyPolicy.Validate(id, Guid.NewGuid(), [Guid.NewGuid(), id], "An item class");

        Assert.Equal("The item class hierarchy cannot contain a cycle.", error);
    }

    [Fact]
    public void Hierarchy_accepts_unrelated_ancestors()
    {
        var error = HierarchyPolicy.Validate(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()], "Equipment");

        Assert.Null(error);
    }

    [Theory]
    [InlineData(false, false, true, null)]
    [InlineData(false, true, false, null)]
    [InlineData(false, true, true, "Item class group flag cannot be changed while it has children or items.")]
    [InlineData(true, false, true, "Item class group flag cannot be changed while it has children or items.")]
    public void Catalog_class_group_flag_protects_dependants(
        bool current, bool requested, bool hasDependants, string? expected)
    {
        Assert.Equal(expected,
            CatalogItemClassPolicy.ValidateGroupFlagChange(current, requested, hasDependants));
    }

    [Fact]
    public void Catalog_class_parent_must_be_active_group_of_same_type()
    {
        Assert.Equal("The parent item class must be active.",
            CatalogItemClassPolicy.ValidateParent(CatalogItemType.Product, null));
        Assert.Equal("The parent item class must be active.",
            CatalogItemClassPolicy.ValidateParent(CatalogItemType.Product,
                new(CatalogItemType.Product, true, false)));
        Assert.Equal("The parent item class must be a group.",
            CatalogItemClassPolicy.ValidateParent(CatalogItemType.Product,
                new(CatalogItemType.Product, false, true)));
        Assert.Equal("The parent item class must have the same item type.",
            CatalogItemClassPolicy.ValidateParent(CatalogItemType.Product,
                new(CatalogItemType.Work, true, true)));
        Assert.Null(CatalogItemClassPolicy.ValidateParent(CatalogItemType.Product,
            new(CatalogItemType.Product, true, true)));
    }

    [Theory]
    [InlineData(false, true, 1, 1, "The department must be active.")]
    [InlineData(true, false, 1, 1, "The storage location kind is invalid.")]
    [InlineData(true, true, 2, 1, "One or more storage location types are invalid.")]
    [InlineData(true, true, 2, 2, null)]
    public void Storage_references_return_expected_error(bool departmentActive, bool kindActive,
        int requestedTypes, int activeTypes, string? expected)
    {
        Assert.Equal(expected, StorageLocationPolicy.ValidateReferences(
            departmentActive, kindActive, requestedTypes, activeTypes));
    }

    [Theory]
    [InlineData(false, true, true, "The department must be active.")]
    [InlineData(true, false, true, "The equipment type must be active.")]
    [InlineData(true, true, false, "The parent equipment must be active.")]
    [InlineData(true, true, true, null)]
    public void Equipment_references_return_expected_error(bool departmentActive, bool typeActive,
        bool parentActive, string? expected)
    {
        Assert.Equal(expected, EquipmentPolicy.ValidateReferences(
            departmentActive, typeActive, parentActive));
    }

    [Fact]
    public void Equipment_state_rejects_non_utc_dates()
    {
        var error = EquipmentPolicy.ValidateState(EquipmentStateCodes.Operational,
            DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Local), null);

        Assert.Equal("State dates must include the UTC offset.", error);
    }

    [Fact]
    public void Equipment_state_end_cannot_overlap_next_event()
    {
        var start = new DateTime(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc);

        var error = EquipmentPolicy.ValidateStateEnd(start, start.AddHours(3), start.AddHours(2));

        Assert.Equal("The state interval overlaps the next state.", error);
    }
}
