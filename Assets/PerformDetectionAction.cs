using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PerformDetection", story: "Use [ObsticleDetector] [TargetDetector] [SeekBehaviour] [Avoidance] and [Data] to crete graph", category: "Action", id: "f64ad9993e66c504ee34662af64cf401")]
public partial class PerformDetectionAction : Action
{
    [SerializeReference] public BlackboardVariable<ObsticleDetector> ObsticleDetector;
    [SerializeReference] public BlackboardVariable<TargetDetector> TargetDetector;
    [SerializeReference] public BlackboardVariable<SeekBehaviour> SeekBehaviour;
    [SerializeReference] public BlackboardVariable<ObstacleAvoidanceBehaviour> Avoidance;
    [SerializeReference] public BlackboardVariable<AI_Data> Data;

    protected override Status OnStart()
    {
        if (ObsticleDetector == null && TargetDetector == null)
        {
            return Status.Failure;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        TargetDetector.Value.Detect(Data);
        ObsticleDetector.Value.Detect(Data);

        float[] danger = new float[8];
        float[] interest = new float[8];

        (danger, interest) = SeekBehaviour.Value.GetSteering(danger, interest, Data);
        (danger, interest) = Avoidance.Value.GetSteering(danger, interest, Data);

        return Status.Running;
    }
}

