using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VectorSaveData
{
    public float X;
    public float Y;
    public float Z;

    public VectorSaveData(Vector3 v)
    {
        X = v.x;
        Y = v.y;
        Z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
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
