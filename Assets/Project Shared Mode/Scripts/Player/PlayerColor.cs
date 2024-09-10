using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    public MeshRenderer MeshRenderer;

    [Networked]
    public Color NetworkedColor { get; set; }
}
