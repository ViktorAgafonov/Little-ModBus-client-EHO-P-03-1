using System.Net;
using System.Net.Sockets;

public class ModbusReader
{
    private int Port { get; set; } = 502;                     //default port number

    private string ServerAddress { get; set; } = "127.0.0.1"; //default ip

    public ModbusReader(string serverAddress, int port)
    {
        ServerAddress = serverAddress;
        Port = port;
    }


    public async Task<(int SerialNumber, float Level, double Value)> ReadData()
    {
        using (var client = new TcpClient(ServerAddress, Port))
        using (var stream = client.GetStream())
        {

            var request = new ModbusTcpRequest
            {
                UnitId = 1,
                FunctionCode = 3,
                StartAddress = 1,
                Quantity = 15
            };

            byte[] requestBytes = request.ToBytes();

#if DEBUG
            Console.WriteLine("TX: {0}", string.Join(" ", requestBytes));
#endif

            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            byte[] responseBytes = new byte[256];

            int bytesRead = await stream.ReadAsync(responseBytes, 0, responseBytes.Length);

            var response = ModbusTcpResponse.FromBytes(responseBytes);

            if (response.ByteCount < request.Quantity * 2) //
            {
                throw new Exception("ERROR! Incomplete response received: only " + response.ByteCount + " bytes received!");
            }

            ushort responseTransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(responseBytes, 0));

#if DEBUG
            //Console.WriteLine("RX: " + BitConverter.ToString(responseBytes));
            // fb 41 (64321) -> 41FB (16891) hmm..
#endif

            int serialNumber = SwapBytes(responseBytes, 14);
            float level = BitConverter.ToSingle(BitConverter.GetBytes(SwapBytes(responseBytes, 1)), 0); double value = CalculateValue(responseBytes);

            return (serialNumber, level, value);
        }
    }
    private int SwapBytes(byte[] responseBytes, int registerStartIndex)
    {
        // Console.WriteLine(responseBytes[registerStartIndex*2 + 1]); //F9-249 65-41            64321
        // Console.WriteLine((responseBytes[registerStartIndex * 2 + 2])); //F9-259 65-41            64321

        return (responseBytes[registerStartIndex * 2 + 2] | (responseBytes[registerStartIndex * 2 + 1] << 8));
    }
    private double CalculateValue(byte[] responseBytes)
    {
        byte[] valueBytes = new byte[] { responseBytes[4], responseBytes[3], responseBytes[2], responseBytes[1] };
        long a = BitConverter.ToInt32(valueBytes, 0);

        byte x = responseBytes[11];
        double b = Math.Pow(10, x - 3);

        return a * b;
    }

}
