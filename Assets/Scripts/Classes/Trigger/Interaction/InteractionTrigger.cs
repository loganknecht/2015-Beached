using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionTrigger : CustomTrigger {
    void Awake() {
    }
    
    // Use this for initialization
    void Start() {
    }
    
    // Update is called once per frame
    void Update() {
    }
    
    public override void Initialize() {
    }
    
    public virtual void Interact(GameObject gameObjectInteracting) {
        Execute(gameObjectInteracting);
    }
}