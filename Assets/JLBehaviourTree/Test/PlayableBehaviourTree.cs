using System.Collections;
using System.Collections.Generic;
using JLBehaviourTree.BehaviourTree;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// A behaviour that is attached to a playable
public class PlayableBehaviourTree : SerializedScriptableObject,IPlayableAsset,IPlayableBehaviour
{
    /*[OdinSerialize]
    public BehaviourTreeData TreeData;*/
    [OdinSerialize]
    public BehaviourTreeData TreeData;


    public void OnGraphStart(Playable playable)
    {
        Debug.Log("OnGraphStart");
        TreeData.OnStart();
    }

    public void OnGraphStop(Playable playable)
    {
        Debug.Log("OnGraphStop");
        TreeData.OnStop();
    }

    public void OnPlayableCreate(Playable playable)
    {
        Debug.Log("OnPlayableCreate");
       
    }

    public void OnPlayableDestroy(Playable playable)
    {
        Debug.Log("OnPlayableDestroy");
        
    }

    public void OnBehaviourPlay(Playable playable, FrameData info)
    {
        Debug.Log("OnBehaviourPlay");
    }

    public void OnBehaviourPause(Playable playable, FrameData info)
    {
        Debug.Log("OnBehaviourPause");
        
    }

    public void PrepareFrame(Playable playable, FrameData info)
    {
        Debug.Log("PrepareFrame");
        
    }


    public void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Debug.Log("ProcessFrame");
        
    }

    public Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        Debug.Log("CreatePlayable");
        return ScriptPlayable<PlayableBehaviourTree>.Create(graph, this);

    }

    public double duration { get; }
    public IEnumerable<PlayableBinding> outputs { get; }
}


