using System;

namespace xasset
{
    [Serializable]
    public class Downloadable
    {
        public string name = string.Empty;
        public string hash;
        public ulong size;
        public string file { get; set; }
    }
}