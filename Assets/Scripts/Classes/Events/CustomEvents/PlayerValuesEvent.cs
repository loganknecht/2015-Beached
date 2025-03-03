﻿using UnityEngine;
using System.Collections;

public class PlayerValuesEvent : CustomEventObject {
    // GameObject is used because the unity inspector wouldn't let me maintain
    // a reference to the Player script. Freaking Unity, I seriously don't know
    // why it doesn't work by assigning the player component.
    public GameObject playerGameObject = null;
    private Acrocatic.Player acrocaticPlayer = null;
    private Player player = null;
    public bool enableController = false;
    public bool zeroOutVelocity = false;
    public bool isDead = false;
    
    // // Use this for initialization
    // protected override void Awake() {
    // }
    
    // // Use this for initialization
    // protected override void Start() {
    // }
    
    // // Update is called once per frame
    // protected override void Update() {
    // }
    
    protected override void Initialize() {
        base.Initialize();
        // sets player reference
        if(playerGameObject == null) {
            playerGameObject = PlayerManager.Instance.GetPlayerGameObject();
            if(playerGameObject == null) {
                Debug.LogError(this.gameObject.transform.GetFullPath() + " needs its 'playerGameObject' reference to be set in the 'PlayerValuesEvent' Script");
            }
        }
        
        player = playerGameObject.GetComponent<Player>();
        if(player == null) {
            Debug.LogError(this.gameObject.transform.GetFullPath() + " needs its 'player' reference to be set in the 'PlayerValuesEvent' Script");
        }
        
        acrocaticPlayer = playerGameObject.GetComponent<Acrocatic.Player>();
        if(acrocaticPlayer == null) {
            Debug.LogError(this.gameObject.transform.GetFullPath() + " needs its 'acrocaticPlayer' reference to be set in the 'PlayerValuesEvent' Script");
        }
    }
    
    public override void ExecuteLogic() {
        player.ToggleController(player.gameObject, enableController);
        if(zeroOutVelocity) {
            player.ZeroOutVelocity();
        }
        
        if(isDead) {
            if(!acrocaticPlayer.IsDead()) {
                acrocaticPlayer.Dead();
            }
        }
    }
}