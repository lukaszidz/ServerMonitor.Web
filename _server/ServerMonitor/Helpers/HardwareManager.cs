using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Models;

namespace ServerMonitor.Helpers
{
    public static class HardwareManager
    {
        private static PerformanceCounter CpuCounter { get; set; }
        private static readonly ComputerInfo ComputerInfo = new ComputerInfo();
        private static readonly DriveInfo DriveInfo = DriveInfo.GetDrives().First(x => x.Name == "C:\\");

        public static Hardware GetHardware()
        {
            var hardware = new Hardware
            {
                Name = ComputerName(),
                Data = new List<Data<double>>
                {
                    new Data<double> {Name = "CPU", Value = CpuUsage()},
                    new Data<double> {Name = "RAM", Value = MemoryUsage(), Text = TotalMemory().ToString()},
                    new Data<double> {Name = "HDD", Value = DiskUsage(), Text = TotalDiskSpace().ToString()}
                }
            };
            return hardware;
        }

        public static List<Hardware> GetAll()
        {
            var hardwareList = new List<Hardware>();
            var config = LinksHelper.GetLinks("hardwareList");

            if (config == null)
            {
                //TO RETHINK: passing value to the response
                //response.Status = Status.Error;
                //response.AddErrorNotification("Configuration of hardwareList missing");
                //return response;
                throw new NullReferenceException("Configuration of hardwareList missing");
            }

            foreach (LinkItem link in config)
            {
                var linkUrl = $"{link.Url.EnsureSlash()}hardware/get";
                var responseItem = ApiClient.Get<Response>(linkUrl);
                if (responseItem.Status == Status.Success)
                {
                    var innerResponse = (Response)responseItem.Data;
                    if (innerResponse.Status == Status.Success)
                    {
                        var data = ((JObject)innerResponse.Data).ToObject<Hardware>();
                        hardwareList.Add(data);
                    }
                }
            }

            return hardwareList;
        }

        private static double CpuUsage()
        {
            if (CpuCounter == null)
            {
                CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            return Math.Round(CpuCounter.NextValue());
        }

        private static double MemoryUsage()
        {
            var availableMemory = ComputerInfo.AvailablePhysicalMemory;
            var allMemory = ComputerInfo.TotalPhysicalMemory;
            var usedMemory = allMemory - availableMemory;
            var usage = usedMemory * 100 / allMemory;

            return Math.Round((double)usage);
        }

        private static double DiskUsage()
        {
            var p = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
            return Math.Round(p.NextValue());
        }

        private static string ComputerName()
        {
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (var item in moc)
            {
                return ((ManagementObject)item).Properties["DNSHostName"].Value.ToString();
            }

            return string.Empty;
        }

        private static ulong TotalMemory()
        {
            return ComputerInfo.TotalPhysicalMemory;
        }

        private static long TotalDiskSpace()
        {
            return DriveInfo.TotalSize;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Hardware
        {
            [JsonProperty("key")]
            public string Name { get; set; }
            [JsonProperty("data")]
            public List<Data<double>> Data { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Data<T>
        {
            [JsonProperty("key")]
            public string Name { get; set; }
            [JsonProperty("value")]
            public T Value { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}