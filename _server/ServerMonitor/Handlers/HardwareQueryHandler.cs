using System.Collections.Generic;
using MediatR;
using ServerMonitor.Models;
using ServerMonitor.Queries;
using System.Configuration;
using ServerMonitor.Helpers;

namespace ServerMonitor.Handlers
{
    public class HardwareQueryHandler : RequestHandler<HardwareQuery, HardwareQueryResult>
    {
        protected override HardwareQueryResult Handle(HardwareQuery request)
        {
            var cacheLifecycle = int.Parse(ConfigurationManager.AppSettings["CacheInSeconds"]);
            if (request.GetAll)
            {
                var hardwareList = CacheManager.GetObjectFromCache("IISApplications", cacheLifecycle, HardwareManager.GetAll);
                return new HardwareQueryResult(hardwareList);
            }
               
            var singleHardware = CacheManager.GetObjectFromCache("IISApplications", cacheLifecycle, HardwareManager.GetHardware);
            var singleHardwareWrapper = new List<HardwareManager.Hardware> { singleHardware };
            return new HardwareQueryResult(singleHardwareWrapper);
        }
    }
}