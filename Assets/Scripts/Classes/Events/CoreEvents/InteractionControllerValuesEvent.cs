﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionControllerValuesEvent : CustomEventObject {
    public InteractionController interactionController = null;
    public List<InteractionTrigger> triggersToAdd = null;
    public List<InteractionTrigger> triggersToRemove = null;
    
    // // Use this for initialization
    // protected overrid void Awake() {
    // base.Awake();
    // }
    
    // // Use this for initialization
    // protected overrid void Start() {
    // base.Start();
    // }
    
    // // Update is called once per frame
    // protected overrid void Update() {
    // base.Update();
    // }
    
    protected override void Initialize() {
        base.Initialize();
        if(interactionController == null) {
            Debug.LogError("The 'interactionController' reference needs to be set in the 'InteractionControllerValuesEvent' Script on " + this.gameObject.name);
        }
        
        if(triggersToAdd == null) {
            triggersToAdd = new List<InteractionTrigger>();
        }
        
        if(triggersToRemove == null) {
            triggersToRemove = new List<InteractionTrigger>();
        }
    }
    
    public override void ExecuteLogic() {
        foreach(InteractionTrigger triggerToAdd in triggersToAdd) {
            interactionController.AddTrigger(triggerToAdd);
        }
        
        foreach(InteractionTrigger triggerToRemove in triggersToRemove) {
            interactionController.RemoveTrigger(triggerToRemove);
        }
    }
}