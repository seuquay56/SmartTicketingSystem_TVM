using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTicketing.Core.Models;

namespace SmartTicketing.Data.Repositories
{
    public interface IStationRepository
    {
        Task<List<Station>> GetActiveStationsAsync();
        Task<Station?> GetStationByIdAsync(int id);
    }
}
