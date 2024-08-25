/*
 *Copyright(C) 2023 by Chief All rights reserved.
 *Unity版本：2023.2.5f1c1 
 *作者:Chief  
 *创建日期: 2024-01-29 
 *模块说明：二进制读写辅助
 *版本: 1.0
*/

using System.IO;

namespace ECS.AsterBinary
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public interface I_Serializable
    {

        public void Write(BinaryWriter writer);

        public void Read(BinaryReader reader);

    }
}