using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Common.Persistence;

internal static class CodeGenerator
{
    public static async Task<string> EnsureCodeAsync(
        string? code,
        IQueryable<string> existingCodes,
        int codeLength,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(code)) return code.Trim();

        var next = await existingCodes.CountAsync(ct) + 1;
        while (true)
        {
            var candidate = next.ToString(CultureInfo.InvariantCulture).PadLeft(codeLength, '0');
            if (!await existingCodes.AnyAsync(x => x == candidate, ct)) return candidate;
            next++;
        }
    }
}
