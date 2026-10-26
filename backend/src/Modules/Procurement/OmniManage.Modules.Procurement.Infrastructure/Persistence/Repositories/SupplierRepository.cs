using Microsoft.EntityFrameworkCore;
using OmniManage.Modules.Procurement.Domain.Abstractions;
using OmniManage.Modules.Procurement.Domain.Suppliers;

namespace OmniManage.Modules.Procurement.Infrastructure.Persistence.Repositories;

public sealed class SupplierRepository(ProcurementDbContext dbContext) : ISupplierRepository
{
    public Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(supplier);

        return dbContext.Suppliers.AddAsync(supplier, cancellationToken).AsTask();
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return dbContext.Suppliers.AnyAsync(supplier => supplier.Code == code, cancellationToken);
    }

    public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Supplier> Suppliers, long TotalItems)> ListAsync(
        SupplierListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Supplier> query = dbContext.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim().ToLowerInvariant();

            query = query.Where(supplier =>
                supplier.Code.ToLower().Contains(term)
                || supplier.Name.ToLower().Contains(term)
                || (supplier.ContactName != null && supplier.ContactName.ToLower().Contains(term))
                || (supplier.Email != null && supplier.Email.ToLower().Contains(term)));
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(supplier => supplier.IsActive == isActive);
        }

        var totalItems = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var suppliers = await ApplySort(query, criteria)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (suppliers, totalItems);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Supplier> ApplySort(
        IQueryable<Supplier> query,
        SupplierListCriteria criteria)
    {
        var option = criteria.Sort.Count == 0
            ? new SupplierSortOption(SupplierSortField.Name, false)
            : criteria.Sort[0];

        IOrderedQueryable<Supplier> ordered = option.Field switch
        {
            SupplierSortField.Code => option.Descending
                ? query.OrderByDescending(supplier => supplier.Code)
                : query.OrderBy(supplier => supplier.Code),
            SupplierSortField.Name => option.Descending
                ? query.OrderByDescending(supplier => supplier.Name)
                : query.OrderBy(supplier => supplier.Name),
            SupplierSortField.CreatedAt => option.Descending
                ? query.OrderByDescending(supplier => supplier.CreatedAt)
                : query.OrderBy(supplier => supplier.CreatedAt),
            _ => query.OrderBy(supplier => supplier.Name),
        };

        return ordered.ThenBy(supplier => supplier.Id);
    }
}
