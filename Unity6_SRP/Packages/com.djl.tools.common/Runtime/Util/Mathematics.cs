using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nanoha.Utils
{
    class Mathematics
    {
        /// <summary>
        /// 计算两点之间弧度
        /// </summary>
        /// <returns></returns>
        public static float CalculateRadianFrom(float x1, float y1, float x2, float y2)
        {
            return (float)Mathf.Atan2(x2 - x1, y2 - y1);
        }
        /// <summary>
        /// 计算两点之间距离的平方
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static float DistanceBetweenTwoPoints(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (dx * dx + dy * dy);
        }
        
    /// <summary>
        /// 根据起始点，方向，距离算出目标点。、
    /// </summary>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="distance"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
	public static float[] calculateTargetPoint(float originX, float originY, float distance, float direction)
	{
		//算出方向向量
		float dirX = Mathf.Sin(direction);
        float dirY = Mathf.Cos(direction);
				
		dirX = originX + dirX * distance;
		dirY = originY + dirY * distance;
		
		return new float[]{dirX, dirY};
	}
    }
}
