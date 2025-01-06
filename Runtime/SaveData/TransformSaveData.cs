using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TransformSaveData
{
    public VectorSaveData Position;
    public VectorSaveData Rotation;

    public TransformSaveData(Transform t)
    {
        Position = new VectorSaveData(t.position);
        Rotation = new VectorSaveData(t.rotation.eulerAngles);
    }
}
