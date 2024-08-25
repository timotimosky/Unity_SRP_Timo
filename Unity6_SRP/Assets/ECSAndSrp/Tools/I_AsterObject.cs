namespace ECS
{
    /// <summary>
    /// 受控的类型（不能是Monobehaviour）；
    /// 需要管理器控制才会正常走生命周期
    /// </summary>
    public interface I_AsterObject
    {

        /// <summary>
        /// 创建
        /// </summary>
        void Start();

        void Update();

        void LateUpdate();

        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();

    }
}