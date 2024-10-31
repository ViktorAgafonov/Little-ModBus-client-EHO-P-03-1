using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

public class ModbusReader
{
    private int Port { get; set; }
    private string ServerAddress { get; set; }

    private ushort currentTransactionId = 0;

    public ModbusReader(string serverAddress, int port)
    {
        ServerAddress = "127.0.0.1";
        Port = 502;
    }


    public async Task<(int SerialNumber, float Level, double Value)> ReadData()
    {
        using (var client = new TcpClient(ServerAddress, Port))
        using (var stream = client.GetStream())
        {
            ushort transactionId = GetNextTransactionId();

            var request = new ModbusTcpRequest
            {
                TransactionId = transactionId,
                UnitId = 1,
                FunctionCode = 3,
                StartAddress = 0,
                Quantity = 16
            };

            byte[] requestBytes = request.ToBytes();
            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            byte[] responseBytes = new byte[256];
            int bytesRead = await stream.ReadAsync(responseBytes, 0, responseBytes.Length);

            var response = ModbusTcpResponse.FromBytes(responseBytes);

            if (response.ByteCount < request.Quantity * 2) //
            {
                throw new Exception("ERROR! Incomplete response received: only " + response.ByteCount + " bytes received!");
            }

            ushort responseTransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(responseBytes, 0));

            if (responseTransactionId != transactionId)
            {
                throw new Exception("Transaction ID mismatch");
            }

#if DEBUG
            Console.WriteLine($"TransactionId: {response.TransactionId}, ProtocolId: {response.ProtocolId}, Length: {response.Length}, UnitId: {response.UnitId}, FunctionCode: {response.FunctionCode}, ByteCount: {response.ByteCount}");
            Console.WriteLine("Register Values: " + string.Join(", ", response.RegisterValues));
#endif

            // вот тут непонятно, какого приходит не то что ожидаю ибо (надо крутить порядок байт младший-старший?):
            // 0:04:19: Accepted connection.
            // 0:04:19: RX: 00 01 00 00 00 06 01 03 00 00 00 10
            // 0:04:19: Sent data: Function code:3.
            // 0:04:19: TX: 00 01 00 00 00 23 01 03 20 dc 95 4e 3d 16 9b 9a 3b 8e 10 00 00 e2 05 00 00 fe 5f 02 00 11 44 12 03 30 10 00 00 fb 41 00 00
            // fb 41 (64321) -> 41FB (16891) !!! должно быть..

            int serialNumber = SwapBytes(responseBytes, 14); 
            float level = BitConverter.ToSingle(BitConverter.GetBytes(SwapBytes(responseBytes, 1)), 0); double value = CalculateValue(responseBytes);

            return (serialNumber, level, value);
        }
    }

    private double CalculateValue(byte[] responseBytes)
    {
        byte[] valueBytes = new byte[] { responseBytes[4], responseBytes[3], responseBytes[2], responseBytes[1] };
        long a = BitConverter.ToInt32(valueBytes, 0);

        byte x = responseBytes[11];
        double b = Math.Pow(10, x - 3);

        return a * b;
    }
    private ushort GetNextTransactionId()
    {
        ushort transactionId = currentTransactionId;

        currentTransactionId = (currentTransactionId == ushort.MaxValue) ? (ushort)0 : (ushort)(currentTransactionId + 1); return transactionId;
    }
    private int SwapBytes(byte[] responseBytes, int registerStartIndex)
    {
        return (responseBytes[registerStartIndex + 1] << 8) | responseBytes[registerStartIndex];
    }
}