using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SmartTicketing.Core.Models;

namespace SmartTicketing.Data.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly List<Station> _seedStations = new()
        {
            new Station { StationId = 1, StationName = "Central",       LineName = "Riverside Line [01]", BaseFare = 0.00m, StopOrder = 0, EstimatedMinutes = 0,  IsActive = true, IsPopular = false },
            new Station { StationId = 2, StationName = "Market Street", LineName = "Riverside Line [01]", BaseFare = 3.00m, StopOrder = 1, EstimatedMinutes = 3,  IsActive = true, IsPopular = true },
            new Station { StationId = 3, StationName = "Civic Square",  LineName = "Riverside Line [01]", BaseFare = 3.00m, StopOrder = 2, EstimatedMinutes = 6,  IsActive = true, IsPopular = false },
            new Station { StationId = 4, StationName = "City Park",     LineName = "Riverside Line [01]", BaseFare = 3.00m, StopOrder = 3, EstimatedMinutes = 9,  IsActive = true, IsPopular = false },
            new Station { StationId = 5, StationName = "Museum",        LineName = "Riverside Line [01]", BaseFare = 3.00m, StopOrder = 4, EstimatedMinutes = 11, IsActive = true, IsPopular = false },
            new Station { StationId = 6, StationName = "Riverside",     LineName = "Riverside Line [01]", BaseFare = 3.00m, StopOrder = 5, EstimatedMinutes = 14, IsActive = true, IsPopular = true },
            new Station { StationId = 7, StationName = "Harbour",       LineName = "Riverside Line [01]", BaseFare = 3.00m, StopOrder = 6, EstimatedMinutes = 18, IsActive = true, IsPopular = true }
        };

        public async Task<List<Station>> GetActiveStationsAsync()
        {
            if (DatabaseHelper.CanConnectToSqlServer())
            {
                try
                {
                    var list = new List<Station>();
                    using var conn = DatabaseHelper.CreateConnection();
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("SELECT StationId, StationName, LineName, BaseFare, StopOrder, EstimatedMinutes, IsActive, IsPopular FROM dbo.Stations WHERE IsActive = 1 ORDER BY StopOrder ASC", conn);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Station
                        {
                            StationId = reader.GetInt32(0),
                            StationName = reader.GetString(1),
                            LineName = reader.GetString(2),
                            BaseFare = reader.GetDecimal(3),
                            StopOrder = reader.GetInt32(4),
                            EstimatedMinutes = reader.GetInt32(5),
                            IsActive = reader.GetBoolean(6),
                            IsPopular = reader.GetBoolean(7)
                        });
                    }
                    if (list.Count > 0) return list;
                }
                catch
                {
                    // Fall back to seed stations
                }
            }

            // In-Memory Seed fallback
            return await Task.FromResult(new List<Station>(_seedStations));
        }

        public async Task<Station?> GetStationByIdAsync(int id)
        {
            var stations = await GetActiveStationsAsync();
            return stations.Find(s => s.StationId == id);
        }
    }
}
