class TEA
{
    readonly static uint[] KP =
		{
			0x243F6A88, 0x95A308D3, 0x13198A2E, 0x03707344,
			0xA4093832, 0x299F31D0, 0x082EFA98, 0xEC7E6C89,
			0x452821E6, 0x38D01377, 0xBE5466CF, 0x34E90C6D,
			0xC0AC29B7, 0xC97A50DD, 0x3F84D4B5, 0xB5470917,
			0x9216D5D9, 0x7979FB1B
		};

    static void code(uint y, uint z, uint[] k, out uint o1, out uint o2)
    {
        uint sum = 0;
        uint delta = 0x9e3779b9;
        uint n = 32;

        while (n-- > 0)
        {
            y += (z << 4 ^ z >> 5) + z ^ sum + k[(sum & 3) % k.Length];
            sum += delta;
            z += (y << 4 ^ y >> 5) + y ^ sum + k[(sum >> 11 & 3) % k.Length];
        }

        o1 = y;
        o2 = z;
    }
    static void decode(uint y, uint z, uint[] k, out uint o1, out uint o2)
    {
        uint n = 32;
        uint sum;
        uint delta = 0x9e3779b9;

        sum = delta << 5;

        while (n-- > 0)
        {
            z -= (y << 4 ^ y >> 5) + y ^ sum + k[(sum >> 11 & 3) % k.Length];
            sum -= delta;
            y -= (z << 4 ^ z >> 5) + z ^ sum + k[(sum & 3) % k.Length];
        }

        o1 = y;
        o2 = z;
    }

    public static byte[] code(byte[] input)
    {
        byte[] output = new byte[8 * (1 + (input.Length - 1) / 8)];
        for (int i = 0; i < input.Length; i += 8)
        {
            uint y = byteToUint(input, i);
            uint z = byteToUint(input, i + 4);
            uint o1, o2;
            code(y, z, KP, out o1, out o2);
            uintToByte(o1, output, i);
            uintToByte(o2, output, i + 4);
        }
        return output;
    }
    public static byte[] decode(byte[] input)
    {
        byte[] output = new byte[input.Length];
        for (int i = 0; i < input.Length; i += 8)
        {
            uint y = byteToUint(input, i);
            uint z = byteToUint(input, i + 4);
            uint o1, o2;
            decode(y, z, KP, out o1, out o2);
            uintToByte(o1, output, i);
            uintToByte(o2, output, i + 4);
        }
        return output;
    }

    static uint byteToUint(byte[] input, int index)
    {
        uint output = 0;
        if (index < input.Length)
            output += ((uint)input[index++]);
        if (index < input.Length)
            output += ((uint)input[index++] << 8);
        if (index < input.Length)
            output += ((uint)input[index++] << 16);
        if (index < input.Length)
            output += ((uint)input[index++] << 24);
        return output;
    }

    static void uintToByte(uint input, byte[] output, int index)
    {
        if (index < output.Length)
            output[index++] = ((byte)((input & 0xFF)));
        if (index < output.Length)
            output[index++] = ((byte)((input >> 8) & 0xFF));
        if (index < output.Length)
            output[index++] = ((byte)((input >> 16) & 0xFF));
        if (index < output.Length)
            output[index++] = ((byte)((input >> 24) & 0xFF));
    }
}