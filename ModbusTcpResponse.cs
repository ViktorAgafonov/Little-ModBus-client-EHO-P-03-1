internal class ModbusTcpResponse
{
    public byte UnitId { get; private set; }
    public byte FunctionCode { get; private set; }
    public byte ByteCount { get; private set; }
    public ushort[] RegisterValues { get; private set; }

    public static ModbusTcpResponse FromBytes(byte[] bytes)
    {
        if (bytes.Length < 9)
        {
            throw new ArgumentException("Response does not contain enough bytes.");
        }

        // hmmm.. 
        var response = new ModbusTcpResponse
        {
            UnitId = bytes[0],
            FunctionCode = bytes[1],
            ByteCount = bytes[2],
        };

        // Extract registers value
        int registerCount = response.ByteCount / 2;

        response.RegisterValues = new ushort[registerCount];

        for (int i = 0; i < registerCount; i++)
        {
            response.RegisterValues[i] = (ushort)bytes[3 + i];
        }

        return response;
    }
}
