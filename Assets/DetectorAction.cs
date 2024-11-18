using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detector", story: "Check [DetectorTarget]", category: "Action", id: "f9b5f248c40f8db291a9acea6db595e1")]
public partial class DetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<TargetDetector> DetectorTarget;

    protected override Status OnStart()
    {
        return DetectorTarget.Value == null ? Status.Failure : Status.Success;
    }
}

