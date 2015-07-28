﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: REMOVE NGUI TRIGGER FUNCTIONALITY OR GIVE THIS A
//      NAMESPACE AND RENAME IT TO "TRIGGER", BECAUSE THE
//      NAME CUSTOM TRIGGER IS DUMB
public class CustomCollision : MonoBehaviour {
    public bool loop = true;
    public int currentIteration = 0;
    
    // Executes these as standard unity hooks
    public List<CustomEventsManager> onEnterEvents = null;
    public List<CustomEventsManager> onStayEvents = null;
    public List<CustomEventsManager> onExitEvents = null;
    
    protected virtual void Awake() {
        Initialize();
    }
    
    // Use this for initialization
    protected virtual void Start() {
    }
    
    // Update is called once per frame
    protected virtual void Update() {
    }
    
    protected virtual void Initialize() {
    }
    
    public virtual void Entered(GameObject gameObjectEntering) {
        // Debug.Log(this.gameObject.name + " entered was triggered.");
        
        // By default the trigger is executed on enter, but in other classes
        // you can override this method in order to determine where you would
        // actually like to execute it
        // FireEnterEvents should occur last in the event logic, but it should
        // occur before the execute logic is run
        FireEnterEvents();
        Execute(gameObjectEntering);
    }
    
    public virtual void Stay(GameObject gameObjectStaying) {
        FireStayEvents();
    }
    
    public virtual void Exited(GameObject gameObjectExiting) {
        FireExitEvents();
    }
    
    public virtual void Execute(GameObject gameObjectToExecute) {
        // Perform only if it's the first iteration, or it should loop
        if(currentIteration < 1
            || loop) {
            ExecuteLogic(gameObjectToExecute);
            currentIteration++;
            
            if(!loop) {
                Collider2D attachedCollider = this.GetComponent<Collider2D>();
                if(attachedCollider != null) {
                    attachedCollider.enabled = false;
                }
            }
        }
    }
    
    public virtual void ExecuteLogic(GameObject gameObjectExecuting) {
    }
    
    public virtual void FireEnterEvents() {
        if(onEnterEvents != null) {
            foreach(CustomEventsManager customEventsManager in onEnterEvents) {
                customEventsManager.Execute();
            }
        }
    }
    
    public virtual void FireStayEvents() {
        if(onStayEvents != null) {
            foreach(CustomEventsManager customEventsManager in onStayEvents) {
                customEventsManager.Execute();
            }
        }
    }
    
    public virtual void FireExitEvents() {
        if(onExitEvents != null) {
            // Debug.Log("Firing exit events");
            foreach(CustomEventsManager customEventsManager in onExitEvents) {
                customEventsManager.Execute();
            }
        }
    }
}