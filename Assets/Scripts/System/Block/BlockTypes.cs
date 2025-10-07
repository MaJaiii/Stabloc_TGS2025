using System.Collections.Generic;
using UnityEngine;

public enum Block3DType
{
    First,
    block01,
    block02,
    block03,
    block04,
    block05,
    block06,
    block07,
    block08,
    block09,
    block10,
    block11,
    block12,
    block13,
    block14,
    block15,
    block16,
    block17,
    block18,
}

public static class PentacubeShapes
{
    public static readonly Dictionary<Block3DType, Vector3[]> Shapes = new()
    {
        [Block3DType.First] = new Vector3[]
        {
            Vector3.zero, Vector3.left, Vector3.right,
            Vector3.forward, Vector3.forward + Vector3.left, Vector3.forward + Vector3.right,
            Vector3.back, Vector3.back + Vector3.right, Vector3.back + Vector3.left
        },
        [Block3DType.block01] = new[] { Vector3.zero, Vector3.back, Vector3.back * 2, Vector3.back * 3, Vector3.forward },
        [Block3DType.block02] = new[] { Vector3.zero, Vector3.forward, Vector3.forward * 2, Vector3.forward * 3, Vector3.right },
        [Block3DType.block03] = new[] { Vector3.zero, Vector3.back, Vector3.right, Vector3.forward, Vector3.forward * 2 },
        [Block3DType.block04] = new[] { Vector3.zero, Vector3.left + Vector3.back, Vector3.left, Vector3.forward, Vector3.forward + Vector3.left },
        [Block3DType.block05] = new[] { Vector3.zero, Vector3.left, Vector3.right, Vector3.back, Vector3.back * 2 },
        [Block3DType.block06] = new[] { Vector3.zero, Vector3.forward, Vector3.forward + Vector3.left, Vector3.forward + Vector3.right, Vector3.forward * 2 },
        [Block3DType.block07] = new[] { Vector3.zero, Vector3.left, Vector3.back, Vector3.back * 2, Vector3.back * 2 + Vector3.right },
        [Block3DType.block08] = new[] { Vector3.zero, Vector3.right, Vector3.right * 2, Vector3.forward, Vector3.forward * 2 },
        [Block3DType.block09] = new[] { Vector3.zero, Vector3.left, Vector3.back, Vector3.back + Vector3.right, Vector3.back * 2 },
        [Block3DType.block10] = new[] { Vector3.zero, Vector3.right, Vector3.back, Vector3.back * 2, Vector3.back * 2 + Vector3.right },
        [Block3DType.block11] = new[] { Vector3.zero, Vector3.up, Vector3.forward, Vector3.forward * 2, Vector3.forward * 2 + Vector3.right },
        [Block3DType.block12] = new[] { Vector3.zero, Vector3.up, Vector3.forward, Vector3.forward * 2, Vector3.forward + Vector3.right },
        [Block3DType.block13] = new[] { Vector3.zero, Vector3.left, Vector3.back, Vector3.up, Vector3.forward },
        [Block3DType.block14] = new[] { Vector3.zero, Vector3.left, Vector3.left + Vector3.back, Vector3.back, Vector3.up },
        [Block3DType.block15] = new[] { Vector3.zero, Vector3.left, Vector3.up, Vector3.forward, Vector3.forward + Vector3.right },
        [Block3DType.block16] = new[] { Vector3.zero, Vector3.left, Vector3.left + Vector3.back, Vector3.left * 2 + Vector3.back, Vector3.up },
        [Block3DType.block17] = new[] { Vector3.zero, Vector3.right, Vector3.up, Vector3.forward, Vector3.forward * 2 },
        [Block3DType.block18] = new[] { Vector3.zero, Vector3.left, Vector3.left + Vector3.back, Vector3.left + Vector3.back * 2, Vector3.up },
    };
}


