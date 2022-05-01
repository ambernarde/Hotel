using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel.Web.Models;

namespace Hotel.Web.Data
{
    public interface IProfilesRepository
    {
        Task<IEnumerable<Profile>> GetAllAsync();

        Task<Profile> GetAsync(int profileId);

        Task AddAsync(Profile newProfile);

        void Update(Profile profile);

        void Remove(int profileId);
    }
}