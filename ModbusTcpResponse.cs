using System;
using System.Net;

internal class ModbusTcpResponse
{
    public byte ByteCount { get; private set; }
    public byte FunctionCode { get; private set; }
    public ushort Length { get; private set; }
    public ushort ProtocolId { get; private set; }
    public ushort TransactionId { get; private set; }
    public byte UnitId { get; private set; }
    public ushort[] RegisterValues { get; private set; }

    public static ModbusTcpResponse FromBytes(byte[] bytes)
    {
        if (bytes.Length < 9)
        {
            throw new ArgumentException("Response does not contain enough bytes.");
        }

        // Создаем и заполняем объект ModbusTcpResponse
        var response = new ModbusTcpResponse
        {
            TransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 0)),
            ProtocolId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 2)),
            Length = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 4)),
            UnitId = bytes[6],
            FunctionCode = bytes[7],
            ByteCount = bytes[8],
        };

        // Извлекаем значения регистров
        int registerCount = response.ByteCount / 2;
        response.RegisterValues = new ushort[registerCount];

        for (int i = 0; i < registerCount; i++)
        {
            response.RegisterValues[i] = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 9 + i * 2));
        }

        return response;
    }
}