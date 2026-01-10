using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace El_Buen_Taco.Data
{
    public sealed class ScopedXmlRepositoryWrapper : IXmlRepository
    {
        private readonly IServiceProvider _root;

        public ScopedXmlRepositoryWrapper(IServiceProvider root) => _root = root ?? throw new ArgumentNullException(nameof(root));

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            using var scope = _root.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<EfCoreXmlRepository>();
            return repo.GetAllElements();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            if (element is null) throw new ArgumentNullException(nameof(element));

            using var scope = _root.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<EfCoreXmlRepository>();
            repo.StoreElement(element, friendlyName);
        }
    }
}       