﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Djl.Utils
{
    public class CachedMonoBehaviour : MonoBehaviour
    {

        public GameObject CachedGameObject
        {
            get
            {
                if (m_CachedGameObj == null)
                    m_CachedGameObj = this.gameObject;
                return m_CachedGameObj;
            }
        }

        public Transform CachedTransform
        {
            get
            {
                return GetCachedComponent<Transform>();
            }
        }

        public T GetCachedComponent<T>() where T : UnityEngine.Component
        {
            System.Type t = typeof(T);
            UnityEngine.Component ret;
            if (!m_CachedCompentMap.TryGetValue(t, out ret))
            {
                GameObject gameObj = CachedGameObject;
                if (gameObj == null)
                    return null;
                T target = gameObj.GetComponent<T>();
                if (target == null)
                    return null;
                m_CachedCompentMap.Add(t, target);
                return target;
            }

            T comp = ret as T;
            return comp;
        }


        private Dictionary<System.Type, UnityEngine.Component> m_CachedCompentMap = new Dictionary<System.Type, Component>();
        private GameObject m_CachedGameObj = null;
    }
}
