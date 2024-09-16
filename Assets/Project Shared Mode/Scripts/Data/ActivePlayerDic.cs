using Fusion;
using UnityEngine;

//! NOT USING
public class ActivePlayerDic : NetworkBehaviour
{
    public static ActivePlayerDic Local { get; set; }

    //*  TESTING PLAYER DATA LIST ACTIVE
    [Networked]
    [Capacity(10)] // Sets the fixed capacity of the collection
    [UnitySerializeField] // Show this private property in the inspector.
    public NetworkDictionary<int, NetworkString<_32>> NetDict => default;
    //*  TESTING PLAYER DATA LIST ACTIVE

    public override void Spawned()
    {
        Local = this;
        DontDestroyOnLoad(this);
    }

}
