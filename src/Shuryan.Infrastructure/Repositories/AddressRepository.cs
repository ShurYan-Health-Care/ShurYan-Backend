using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// تأكد من أن الـ namespace صحيح حسب هيكل مشروعك
namespace Shuryan.Infrastructure.Repositories
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        private readonly ShuryanDbContext _context;

        public AddressRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetByCityAsync(string city)
        {
            var normalizedCity = city.Trim().ToLower();

            return await _context.Addresses
                .Where(a => a.City.ToLower() == normalizedCity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetByGovernorateAsync(Governorate governorate)
        {
            return await _context.Addresses
                .Where(a => a.Governorate == governorate)
                .ToListAsync();
        }
    }
}