using MediatR;
using ServerMonitor.Models;

namespace ServerMonitor.Queries
{
    public class HardwareQuery : IRequest<HardwareQueryResult>
    {
        public HardwareQuery()
        {

        }

        public HardwareQuery(bool getAll)
        {
            GetAll = getAll;
        }

        public bool GetAll { get; }
    }
}