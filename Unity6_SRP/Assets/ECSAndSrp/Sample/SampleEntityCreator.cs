/*
 *Copyright(C) 2023 by Chief All rights reserved.
 *Unity版本：2022.2.0b14 
 *作者:Chief  
 *创建日期: 2022-12-07 
 *模块说明：示例创建器
 *版本: 1.0
*/
using Unity.Entities;
using UnityEngine;

namespace ECS.Sample
{
    /// <summary>
    /// 测试用的，创建一个Entity；
    /// </summary>
    public class SampleEntityCreator : MonoBehaviour
    {

        void Start()
        {
            CreateEntity();
            
        }


        public void CreateEntity()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, new ASampleData_ID(999));
        }

    }
}