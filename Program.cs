class Program
{
    static async Task Main()
    {
        string ServerAddress = "127.0.0.1";
        int Port = 502;

        ModbusReader modbusReader = new(ServerAddress, Port);

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
