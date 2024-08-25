using System.IO;
using System.Threading.Tasks;

namespace ECS.AsterTask
{
    /// <summary>
    /// 多线程辅助:异步的文件读写接口
    /// </summary>
    public interface I_AsyncSerializable
    {

        public Task Write(BinaryWriter writer);

        public Task Read(BinaryReader reader);

    }
}