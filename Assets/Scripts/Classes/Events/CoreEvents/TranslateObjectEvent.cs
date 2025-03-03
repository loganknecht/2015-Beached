﻿using UnityEngine;
using System.Collections;

public class TranslateObjectEvent : CustomEventObject {
    // public GoTween translationTween = null;
    public GameObject objectToTranslate = null;
    public Transform targetPosition = null;
    public Vector3 offset = Vector3.zero;
    public float delay = 0f;
    public float duration = 1f;
    public bool useLocal = false;
    public bool excludeXComponent = false;
    public bool excludeYComponent = false;
    public bool excludeZComponent = false;
    public GoEaseType easingType = GoEaseType.Linear;
    
    // Use this for initialization
    // protected override void Start() {
    // base.Start();
    // }
    
    // // Update is called once per frame
    // protected override void Update() {
    // base.Update();
    // }
    
    // public override void Execute() {
    // base.Execute();
    // }
    
    protected override void Initialize() {
        base.Initialize();
        if(objectToTranslate == null) {
            this.gameObject.LogComponentError("objectToTranslate", this.GetType());
        }
        
        if(targetPosition == null) {
            this.gameObject.LogComponentError("targetPosition", this.GetType());
        }
    }
    
    public override void Execute() {
        FireStartEvents();
        ExecuteLogic();
    }
    
    public override void ExecuteLogic() {
        GoTweenConfig tweenConfig = new GoTweenConfig();
        Vector3 finalPosition = Vector3.zero;
        
        //-----------------------------------------
        // Translate Tween
        //-----------------------------------------
        if(useLocal) {
            finalPosition = new Vector3((excludeXComponent) ? objectToTranslate.transform.localPosition.x : targetPosition.localPosition.x,
                                        (excludeYComponent) ? objectToTranslate.transform.localPosition.y : targetPosition.localPosition.y,
                                        (excludeZComponent) ? objectToTranslate.transform.localPosition.z : targetPosition.localPosition.z);
            finalPosition += offset;
            tweenConfig.localPosition(finalPosition);
        }
        else {
            finalPosition = new Vector3((excludeXComponent) ? objectToTranslate.transform.position.x : targetPosition.position.x,
                                        (excludeYComponent) ? objectToTranslate.transform.position.y : targetPosition.position.y,
                                        (excludeZComponent) ? objectToTranslate.transform.position.z : targetPosition.position.z);
            finalPosition += offset;
            tweenConfig.position(finalPosition);
        }
        
        tweenConfig.setDelay(delay)
        .setEaseType(easingType)
        .onComplete(complete => {
            FireFinishEvents();
        });
        
        objectToTranslate.AddGoTween(Go.to(objectToTranslate.transform,
                                           duration,
                                           tweenConfig));
    }
}
