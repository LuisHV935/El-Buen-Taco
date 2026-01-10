using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;

namespace El_Buen_Taco.Data;

public class EfCoreXmlRepository : IXmlRepository
{
    private readonly DataProtectionKeysContext _context;

    public EfCoreXmlRepository(DataProtectionKeysContext context)
    {
        _context = context;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        // Orden por Id para determinismo
        return _context.DataProtectionKeys
            .AsNoTracking()
            .OrderBy(k => k.Id)
            .Select(k => XElement.Parse(k.Xml))
            .ToList()
            .AsReadOnly();
    }

    public void StoreElement(XElement element, string? friendlyName)
    {
        var xml = element.ToString(SaveOptions.DisableFormatting);
        var entity = new DataProtectionKeyEntity
        {
            Xml = xml,
            FriendlyName = friendlyName
        };
        _context.DataProtectionKeys.Add(entity);
        _context.SaveChanges();
    }
}