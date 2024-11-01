internal class ModbusTcpRequest
{
    public byte UnitId { get; set; }
    public byte FunctionCode { get; set; }
    public int StartAddress { get; set; }
    public int Quantity { get; set; }

    public ModbusTcpRequest()
    {
        UnitId = 1;
        FunctionCode = 3;
        StartAddress = 1;
        Quantity = 1;
    }

    public byte[] ToBytes()
    {
        byte[] bytes = new byte[8];

        bytes[0] = UnitId;
        bytes[1] = FunctionCode;
        bytes[2] = (byte)(StartAddress >> 8);
        bytes[3] = (byte)(StartAddress & 0xFF);
        bytes[4] = (byte)(Quantity >> 8);
        bytes[5] = (byte)(Quantity & 0xFF);

        ushort crc = CalculateCRC(bytes, 6);
        bytes[6] = (byte)crc;
        bytes[7] = (byte)(crc >> 8);

        return bytes;
    }

    private ushort CalculateCRC(byte[] data, int length)
    {
        const ushort polynom = 0xA001; //for modbus polunnomes...
        ushort crc = 0xFFFF;

        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];

            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x001) != 0) crc = (ushort)((crc >> 1) ^ polynom);
                else crc >>= 1;
            }
        }
        return crc;
    }
}
