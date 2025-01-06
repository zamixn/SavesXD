using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuaternionSaveData
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public QuaternionSaveData(Quaternion q)
    {
        X = q.x;
        Y = q.y;
        Z = q.z;
        W = q.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(X, Y, Z, W);
    }

    public string ToJson()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }

    public static VectorSaveData FromJson(string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<VectorSaveData>(json);
    }
}
