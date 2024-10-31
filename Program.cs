using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Transactions;

class Program
{
    static async Task Main()
    {
        int Port = 502;
        string ServerAddress = "127.0.0.1";

        ModbusReader modbusReader = new ModbusReader(ServerAddress, Port);
      
        while (true)
        {
            try
            {
                var data = await modbusReader.ReadData();
                Console.WriteLine($"SerialNumber: {data.SerialNumber}, Level: {data.Level}, Value: {data.Value}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}