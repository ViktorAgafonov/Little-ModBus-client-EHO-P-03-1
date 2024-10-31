using System.Net;

internal class ModbusTcpRequest
{
    public byte FunctionCode { get; set; }
    public ushort Length { get; set; }
    public ushort ProtocolId { get; set; }
    public ushort Quantity { get; set; }
    public ushort StartAddress { get; set; }
    public ushort TransactionId { get; set; }
    public byte UnitId { get; set; }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[12];
        Array.Copy(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)TransactionId)), 0, bytes, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)ProtocolId)), 0, bytes, 2, 2);
        Array.Copy(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)Length)), 0, bytes, 4, 2);
        bytes[6] = UnitId;
        bytes[7] = FunctionCode;
        Array.Copy(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)StartAddress)), 0, bytes, 8, 2);
        Array.Copy(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)Quantity)), 0, bytes, 10, 2);
        return bytes;
    }

    public ModbusTcpRequest()
    {
        
        ProtocolId = 0;
        Length = 6;
        UnitId = 1;
        FunctionCode = 3;
        StartAddress = 1;
        Quantity = 1;
    }
}