using UnityEngine;
using System.Collections;

public class InuRandom
{
	static InuRandom sInstance;
	
	public static InuRandom instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new InuRandom();
			}
			return sInstance;
		}
	}
	
	public InuRandom()
	{
		seed = (int)System.DateTime.Now.Ticks;
	}
	
	int _seed = 0;
	bool _random = true;
	
	// the seed of random
	public int seed
	{
		set
		{
            Random.InitState(_seed);
            _seed = value;
		}
		get
		{
			return _seed;
		}
	}
	
	// enable or diable random
	public bool random
	{
		set
		{
			_random = value;
		}
	}
	
	public float Float(float min, float max)
	{
		if (_random)
			return Random.Range(min, max);
		else
			return min;
	}
	
	public int Int(int min, int max)
	{
		if (_random)
			return Random.Range(min, max);
		else
			return min;
	}

	public void Bytes(byte[] data)
	{
		int bytesGenerated = 0;
		while (bytesGenerated < data.Length) {
			// Generate a new random number and turn it into a byte array.
			int randInt = Random.Range(int.MinValue, int.MaxValue);
			byte[] randBytes = System.BitConverter.GetBytes(randInt);

			// Fill the data with random bytes.
			for (int i=0; i<randBytes.Length; i++) {
				data[bytesGenerated++] = randBytes[i];
				if (bytesGenerated >= data.Length) {
					// Done filling up data, break early.
					break;
				}
			}
		}
	}
}

public static class InuAIRandom
{
    static uint m_w = 1234;    /* must not be zero, nor 0x464fffff */
    static uint m_z = 4321;    /* must not be zero, nor 0x9068ffff */

    public static uint seed
    {
        set
        {
            m_w = value;
            m_z = ~value;
        }
    }

    static uint get_random()
    {
        m_z = 36969 * (m_z & 65535) + (m_z >> 16);
        m_w = 18000 * (m_w & 65535) + (m_w >> 16);
        uint result = (m_z << 16) + m_w;  /* 32-bit result */
        //if (InuReplayDebug.ENABLED)
        //{
        //    if (InuTime.IsPlayingFromReplayData() || InuTime.IsRecording())
        //        InuReplayDebug.AddRandom(result, System.Environment.StackTrace);
        //}
        return result;
    }
    public static int Int(int min, int max)
    {
        if (max <= min) return min;

        int random = (int)get_random();
        random = random < 0 ? -random : random;
        return min + random % (max - min);
    }
    public static uint UInt(uint min, uint max)
    {
        if (max <= min) return min;

        uint random = get_random();
        return min + random % (max - min);
    }
    public static long Long(long min, long max)
    {
        int left1 = (int)(min >> 32);
        int left2 = (int)(max >> 32);

        if ((left1 == 0 || left1 == -1) && (left2 == 0 || left2 == -1))
        {
			return Int((int)min, (int)max);
        }
        else
        {

            long left = Int(left1, left2) << 32;

            uint right = get_random();

            return left | right;
        }
    }
//    public static InuFloat Float(InuFloat min, InuFloat max)
//    {
//        InuFloat result = new InuFloat();
//        result.ScaledSet(Long(min.i, max.i));
//        return result;
//    }
}
