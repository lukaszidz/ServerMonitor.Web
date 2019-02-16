using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Http;
using MediatR;
using Microsoft.VisualBasic.Devices;
using ServerMonitor.Helpers;
using ServerMonitor.Models;
using ServerMonitor.Queries;

namespace ServerMonitor.Controllers
{
    public class HardwareController : BaseApi
    {
        protected static PerformanceCounter CpuCounter { get; set; }

        private readonly ComputerInfo _computerInfo = new ComputerInfo();
        private readonly DriveInfo _driveInfo = DriveInfo.GetDrives().First(x => x.Name == "C:\\");
        private readonly IMediator _mediator;

        public HardwareController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("Hardware/Get")]
        public Response Get()
        {
            var response = new Response();
            try
            {
                response.Data = _mediator.Send(new HardwareQuery()).Result;
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                response.Status = Status.Error;
                response.AddErrorNotification(ex.Message, ex.StackTrace);
                return response;
            }
        }

        [HttpGet]
        [Route("Hardware/GetAll")]
        public Response GetAll()
        {
            var response = new Response();
            response.Data = _mediator.Send(new HardwareQuery(getAll: true)).Result;
            return response;
        }
    }
}