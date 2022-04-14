using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;
using TMPro;

public class PlayerDefaults : MonoBehaviour
{
    Action<PlayerDefaultInfo> finishedCallback;
    public TMP_InputField nameInput;

    [Serializable]
    public class PlayerDefaultInfo : INetworkSerializable
    {
        public string name;
        public Color color;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref color);
        }
    }
    public void setup(Action<PlayerDefaultInfo> finishedCallback)
    {
        this.finishedCallback = finishedCallback;
    }

    public void setColor(Color c)
    {
        PlayerDefaultInfo info = new PlayerDefaultInfo();
        info.color = c;
        info.name = nameInput.text;

        finishedCallback(info);
        //Destroy(this.gameObject);
    }
}
