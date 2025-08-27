using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockAction))]
public class BlockActionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BlockAction blockAction = target as BlockAction;

        if (GUILayout.Button("Generate Block") && Application.isPlaying)
        {
            blockAction.GenerateBlock(blockAction.blockIndex);
        }
    }
}
