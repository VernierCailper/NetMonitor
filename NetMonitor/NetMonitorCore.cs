using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NetMonitor
{
    
    static class NetMonitorCore
    {

        private static PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
        private static string instance = performanceCounterCategory.GetInstanceNames()[0];//考虑添加自动便利功能自动选择网卡
       
        private static PerformanceCounter performanceCounterRecv = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
        private static PerformanceCounter performanceCounterSend = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);

        public static float GetNetRecv()
        {
            return performanceCounterRecv.NextValue();
        }

        public static float GetNetSend()
        {
            return performanceCounterSend.NextValue();
        }
    }
   
}