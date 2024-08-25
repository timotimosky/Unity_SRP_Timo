using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ECS
{
    /// <summary>
    /// Ecs通用工具
    /// </summary>
    public static class AsterUtils
    {
        // 系统时间辅助
        public static string TimeNowMMSSFF => DateTime.Now.ToString("mm:ss:fff");

  
        #region 可以给多线程取值的部分

        public static float GameTime => GameMgr.GameTime;

        public static int GameFrameCount => GameMgr.GameFrameCount;

        public static int TargetFrameRate => GameMgr.TargetFrameRate;

        public static float FPS => GameMgr.FPS;

        #endregion

        #region Unity组件辅助
        public static GameObject CreateGameObjectAtPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var obj = GameObject.Find(path);
            if (obj == null)
            {
                //在最近父节点进行创建；
                var index = path.LastIndexOf('/');
                if (index == -1)
                    return new GameObject(path);

                var parentPath = path.Substring(0, index);
                var selftPath = path.Substring(index + 1);

                obj = new GameObject(selftPath);

                var parent = CreateGameObjectAtPath(parentPath);
                if (parent != null)
                    obj.transform.parent = parent.transform;

                NormalizeGameObject(obj);
            }

            return obj;
        }

        public static void NormalizeGameObject(GameObject target)
        {
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
        }

        public static void SetParent(this GameObject obj, Component target, bool resetTrans = false)
        {
            if (obj == null || target == null)
                return;

            obj.transform.SetParent(target.transform);
            if (resetTrans)
            {
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
            }
        }

        public static void SetParent(this GameObject obj, GameObject target, bool resetTrans = false)
        {
            if (target == null)
                return;

            obj?.SetParent(target.transform, resetTrans);
        }

        public static void SetActiveSafe(this GameObject obj, bool active)
        {
            if (obj == null)
                return;

            if (obj.activeSelf == active)
                return;

            obj.SetActive(active);
        }

        public static void SetActiveSafe(this Component compT, bool active)
        {
            if (compT == null)
                return;

            compT.gameObject.SetActiveSafe(active);
        }

        public static void Destory(this Object obj)
        {
            if (obj == null)
                return;

            //特殊：于Transform的判定；
            if (obj is Transform)
                (obj as Transform).gameObject.Destory();

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Object.DestroyImmediate(obj);
            else
#endif
                Object.Destroy(obj);
        }

        /// <summary>
        /// 获取层级名；
        /// </summary>
        public static string GetHierarchyName(this Transform trans)
        {
            if (trans == null)
                return null;

            string name = trans.name;
            var parent = trans.parent;
            while (parent != null)
            {
                name = $"{parent.name}/{name}";
                parent = parent.parent;
            }
            return name;
        }

        /// <summary>
        /// 获取根节点
        /// </summary>
        /// <returns></returns>
        public static Transform GetRootTransform(this Transform trans)
        {
            if (trans == null)
                return null;
            if (trans.parent == null)
                return trans;
            return trans.parent.GetRootTransform();
        }

        #endregion

        #region 数学计算

        /// <summary>
        /// 计算直线与平面的交点
        /// 
        /// 通用的部分；
        /// 
        /// ——程一峰；2020.11.09
        /// </summary>
        /// <param name="point">直线上某一点</param>
        /// <param name="direct">直线的方向</param>
        /// <param name="planeNormal">垂直于平面的的向量</param>
        /// <param name="planePoint">平面上的任意一点</param>
        /// <returns></returns>
        public static Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
            //直线与平面的交点
            Vector3 hitPoint = (d * direct.normalized) + point;
            return hitPoint;
        }

        /// <summary>
        /// 点到直线的距离；
        /// </summary>
        /// <param name="point">目标点</param>
        /// <param name="linePoint1">直线上一点</param>
        /// <param name="linePoint2">直线上另一点</param>
        /// <returns></returns>
        public static float GetDistancePointToLine(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
        {
            Vector3 vec1 = point - linePoint1;
            Vector3 vec2 = linePoint2 - linePoint1;
            Vector3 vecProj = Vector3.Project(vec1, vec2);
            float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
            return dis;
        }

        /// <summary>
        /// 
        /// 计算射线与平面的交点
        /// 
        /// 通用的部分；
        /// 
        /// ——程一峰；2020.11.17
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="planeNormal">垂直于平面的的向量</param>
        /// <param name="planePoint">平面上的任意一点</param>
        /// <returns></returns>
        public static Vector3 GetIntersectWithLineAndPlane(Ray ray, Vector3 planeNormal, Vector3 planePoint) { return GetIntersectWithLineAndPlane(ray.origin, ray.direction, planeNormal, planePoint); }

        /// <summary>
        /// 
        /// 计算射线与水平平面的交点
        /// 
        /// 通用的部分；
        /// 
        /// ——程一峰；2020.11.17
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static Vector3 GetIntersectWithLineAndPlane(Ray ray, float height)
        {
            var point = GetIntersectWithLineAndPlane(ray.origin, ray.direction, Vector3.up, new Vector3(0, height, 0));
            point.y = height;
            return point;
        }

        /// <summary>
        /// 将值限制在两个Vector3之间
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector3 Clamp(Vector3 pos, Vector3 min, Vector3 max)
        {
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            pos.z = Mathf.Clamp(pos.z, min.z, max.z);
            return pos;
        }

        /// <summary>
        /// 将值限制在两个Vector3之间
        /// </summary>
        public static Vector3 ClampNoY(Vector3 pos, Vector3 min, Vector3 max)
        {
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.z = Mathf.Clamp(pos.z, min.z, max.z);
            return pos;
        }

        /// <summary>
        /// 将值限制在两个Vector2之间
        /// 不修改Y轴；
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="min">相当于XZ</param>
        /// <param name="max">相当于XZ</param>
        /// <returns></returns>
        public static Vector3 ClampByVec2_NoY(Vector3 pos, Vector2 min, Vector2 max)
        {
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.z = Mathf.Clamp(pos.z, min.y, max.y);
            return pos;
        }

        /// <summary>
        /// 将值限制在两个Vector2之间
        /// 不修改Z轴；
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="min">相当于XY</param>
        /// <param name="max">相当于XY</param>
        /// <returns></returns>
        public static Vector3 ClampByVec2_NoZ(Vector3 pos, Vector2 min, Vector2 max)
        {
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            return pos;
        }

        /// <summary>
        /// 将值限制在两个Vector2之间
        /// </summary>
        /// <param name="pos">当前值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static Vector2 Clamp(Vector2 pos, Vector2 min, Vector2 max)
        {
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            return pos;
        }

        /// <summary>
        /// 将值限制在两个Vector2之间
        /// </summary>
        /// <param name="pos">当前值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static Vector2Int Clamp(Vector2Int pos, Vector2Int min, Vector2Int max)
        {
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            return pos;
        }

        /// <summary>
        /// 获取角度；
        /// Z旋转，默认朝上；
        /// 一般给UI用；
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float GetAngleZ_Up(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            float angle = Vector2.Angle(dir, Vector2.up);

            if (dir.x > 0)
                angle = 90 - angle;
            else
                angle = 90 + angle;
            return angle;
        }

        /// <summary>
        /// 获取角度；
        /// Y旋转，给3D平面单位；
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float GetAngleY_Up(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            float angle = Vector3.Angle(dir, Vector3.up);
            return angle;
        }

        public static Vector3 Lerp(Vector3 from, Vector3 to, float p)
        {
            from.x = Mathf.Lerp(from.x, to.x, p);
            from.y = Mathf.Lerp(from.y, to.y, p);
            from.z = Mathf.Lerp(from.z, to.z, p);
            return from;
        }

        public static Vector3 SmoothStep(Vector3 from, Vector3 to, float p)
        {
            from.x = Mathf.SmoothStep(from.x, to.x, p);
            from.y = Mathf.SmoothStep(from.y, to.y, p);
            from.z = Mathf.SmoothStep(from.z, to.z, p);
            return from;
        }

        /// <summary>
        /// 距离的平方；
        /// </summary>
        public static float DistanceSq(Vector3 a, Vector3 b)
        {
            float xSq = (a.x - b.x) * (a.x - b.x);
            float ySq = (a.y - b.y) * (a.y - b.y);
            float zSq = (a.z - b.z) * (a.z - b.z);
            return xSq + ySq + zSq;
        }

        /// <summary>
        /// 距离的平方；
        /// </summary>
        public static float DistanceSq(Vector2 a, Vector2 b)
        {
            float xSq = (a.x - b.x) * (a.x - b.x);
            float ySq = (a.y - b.y) * (a.y - b.y);
            return xSq + ySq;
        }

        /// <summary>
        ///计算两个点的距离平方
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static float SqrDistance(Vector2 vector1, float2 data2)
        {
            return ((vector1.x - data2.x) * (vector1.x - data2.x)) + ((vector1.y - data2.y) * (vector1.y - data2.y));
        }

        /// <summary>
        /// 获取坐标差值之和；
        /// 忽略Y轴；
        /// 这个计算值一定大于等于其实际距离；
        /// </summary>
        public static float Distance_FastByCoDiff_NoY(Vector3 v1, Vector3 v2)
        {
            float x = v1.x - v2.x;
            if (x < 0)
                x = -x;

            float z = v1.z - v2.z;
            if (z < 0)
                z = -z;

            return x + z;
        }

        /// <summary>
        /// 使一个值与目标值同号；
        /// 如果a b 同号，返回原值；
        ///如果a b 不同号，则返回0 ；
        ///若b为0，则也返回0；
        ///如设定了默认值，则用默认值代替0；
        /// </summary>
        /// <param name="v"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float MakeSame_PN(float a, float b, float defV = 0)
        {
            if (a * b > 0)
                return a;
            return defV;
        }

        /// <summary>
        /// 粗略的检查两点间的距离是否大于指定距离
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool CheckOutSide_Rough(Vector2 data1, int2 data2, int distance)
        {
            if (Mathf.Abs(data1.x - data2.x) > distance)
            {
                return true;
            }
            if (Mathf.Abs(data1.y - data2.y) > distance)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据目标值扩充Vec2，一般用于扩充边界；
        /// </summary>
        public static Vector2 GetMinMax(Vector2 oldVal, float val)
        {
            oldVal.x = Mathf.Min(oldVal.x, val);
            oldVal.y = Mathf.Max(oldVal.y, val);
            return oldVal;
        }

        /// <summary>
        /// 根据原值正负返回最大的整数
        /// </summary>
        /// <param name="oldVal"></param>
        /// <returns>正数取较大值；负数取较小值</returns>
        public static int CeilToInt2(float oldVal)
        {
            int ret = Mathf.CeilToInt(oldVal);
            if (oldVal < 0)
                ret -= 1;
            return ret;
        }


        /// <summary>
        /// 抛物线解算器；
        /// 通过不同X轴的三个点计算出抛物线：Y=aX^2+bX+c;
        /// </summary>
        /// <returns>XYZ分别对应abc</returns>
        public static Vector3 ParabolaResolver(Vector2 P1, Vector2 P2, Vector2 P3)
        {
            if (P1.x == P2.x || P1.x == P3.x || P2.x == P3.x)
            {
               DebugTool.LogError($"参数错误，三个点的X值应互不相等！");
                return Vector3.zero;
            }

            float denominator = (P2.x - P3.x) * (P1.x - P2.x) * (P1.x - P3.x);
            float a = -(((P2.y - P3.y) * P1.x) - ((P2.x - P3.x) * P1.y) + (P2.x * P3.y) - (P3.x * P2.y)) / denominator;
            float b = (((P2.y - P3.y) * P1.x.Square()) + (P2.x.Square() * P3.y) - (P3.x.Square() * P2.y) - ((P2.x.Square() - P3.x.Square()) * P1.y)) / denominator;
            float c = ((((P2.x * P3.y) - (P3.x * P2.y)) * P1.x.Square()) - (((P2.x.Square() * P3.y) - (P3.x.Square() * P2.y)) * P1.x) + (((P2.x.Square() * P3.x) - (P2.x * P3.x.Square())) * P1.y)) / denominator;
            return new Vector3(a, b, c);
        }

        /// <summary>
        /// 抛物线解算，Y=aX^2+bX+c;
        /// </summary>
        /// <param name="param">XYZ分别对应abc</param>
        /// <param name="x">X轴</param>
        /// <returns></returns>
        public static float ParabolaCaculator(Vector3 param, float x) { return (param.x * x.Square()) + (param.y * x) + param.z; }


        /// <summary>
        /// 通过三个点获取圆的解析式；
        /// </summary>
        /// <returns>（x-a）^2+（y-b）^2=r^2</returns>
        public static Vector3 CircleResolver(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            //在一条直线上则不能形成圆形；
            if ((p2 - p1).normalized == (p3 - p2).normalized)
                return Vector3.zero;

            //计算圆形；
            float h = 2 * (p2.x - p1.x);
            float f = 2 * (p2.y - p1.y);
            float g = (p2.x * p2.x) - (p1.x * p1.x) + (p2.y * p2.y) - (p1.y * p1.y);
            float a = 2 * (p3.x - p2.x);
            float b = 2 * (p3.y - p2.y);
            float c = (p3.x * p3.x) - (p2.x * p2.x) + (p3.y * p3.y) - (p2.y * p2.y);
            float Col = ((g * b) - (c * f)) / ((h * b) - (a * f));
            float Row = ((a * g) - (c * h)) / ((a * f) - (b * h));
            float Rad = Mathf.Sqrt(((Col - p1.x) * (Col - p1.x)) + ((Row - p1.y) * (Row - p1.y)));

            return new Vector3(Col, Row, Rad);
        }

        /// <summary>
        /// 通过两点式的圆解析方程求解；
        /// </summary>
        /// <param name="param">（x-a）^2+（y-b）^2=r^2</param>
        /// <param name="x"></param>
        /// <returns>返回两个解</returns>
        public static Vector2 CircleCaculator(Vector3 param, float x)
        {
            float a = param.z.Square() / (x - param.x).Square();
            float b = Mathf.Sqrt(a);
            return new Vector2(-b + param.y, b + param.y);
        }

        /// <summary>
        /// 获取朝向
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <param name="isValid"></param>
        /// <param name="ignoreY"></param>
        /// <returns></returns>
        public static Vector3 GetOrientation(Vector3 vectorA, Vector3 vectorB, out bool isValid, bool ignoreY = true)
        {
            isValid = true;
            var forward = vectorA - vectorB;
            if (ignoreY == true)
            {
                forward.y = 0;
            }

            if (forward == Vector3.zero)
            {
                isValid = false;
                forward = Vector3.forward;
            }

            return forward.normalized;
        }

        /// <summary>
        /// 获取旋转
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public static Quaternion GetRotation(Vector3 velocity)
        {
            return Quaternion.FromToRotation(Vector3.forward, velocity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(float2 tPos, float2 min, float size)
        {
            return tPos.x >= min.x && tPos.y >= min.y
                && tPos.x < (min.x + size) && tPos.y < (min.y + size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(float2 tPos, float2 min, float2 size)
        {
            return tPos.x >= min.x && tPos.y >= min.y
                && tPos.x < (min.x + size.x) && tPos.y < (min.y + size.y);
        }

        /// <summary>
        /// 平方
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Square(this float value)
        {
            return value * value;
        }

        public static Vector2 XZ(this Vector3 pos)
        {
            return new Vector2(pos.x, pos.z);
        }

        public static Vector2 XY(this Vector3 pos)
        {
            return new Vector2(pos.x, pos.y);
        }

        public static Vector2 ZY(this Vector3 pos)
        {
            return new Vector2(pos.z, pos.y);
        }

        /// <summary>
        /// 看向某个目标时，旋转的欧拉角
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target">目标点</param>
        /// <returns>旋转欧拉角</returns>
        public static Vector3 LookAtEulerAngle(this Vector3 pos, Vector3 target)
        {
            Vector3 fwd = target - pos;
            return Quaternion.LookRotation(fwd, Vector3.up).eulerAngles;
        }

        public static int ToInt(this long val)
        {
            //极值判定；
            if (val > int.MaxValue)
            {
               DebugTool.LogError($"传入值过大： {val}");
                return int.MaxValue;
            }

            if (val < int.MinValue)
            {
               DebugTool.LogError($"传入值过小： {val}");
                return int.MinValue;
            }

            return System.Convert.ToInt32(val);
        }

        public static int Add(this int a, long b)
        {
            long addRet = a + b;
            return addRet.ToInt();
        }


        /// <summary>
        /// 是否是有效的 Vector3；
        /// </summary>
        public static bool IsVaild(this Vector3 point)
        {
            if (float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsNaN(point.z))
                return false;

            if (float.IsInfinity(point.x) || float.IsInfinity(point.y) || float.IsInfinity(point.z))
                return false;

            return true;
        }

        public static Vector3 DivByVector3(this Vector3 a, Vector3 b)
        {
            float x = a.x / b.x;
            float y = a.y / b.y;
            float z = a.z / b.z;
            return new Vector3(x, y, z);
        }

        public static Vector3 MulByVector3(this Vector3 a, Vector3 b)
        {
            float x = a.x * b.x;
            float y = a.y * b.y;
            float z = a.z * b.z;
            return new Vector3(x, y, z);
        }

        #endregion

    }
}