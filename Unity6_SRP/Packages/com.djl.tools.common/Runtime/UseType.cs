using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UseType
{
    Once =0,
    Scene = 1, //默认,不做任何管理
    MultiInstance = 2, //实例对象池，随着时间销毁 ：如果有多个对象，则使用
    ever = 3,//实例对象池，除非主动销毁，否则不销毁：如果有多个对象，并确定一直重复使用,则使用
}
