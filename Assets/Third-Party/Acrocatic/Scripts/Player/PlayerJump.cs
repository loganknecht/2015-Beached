﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

namespace Acrocatic {
    // Class that controls the jumping mechanic for the player.
    public class PlayerJump : MonoBehaviour {
        // Enums for the PlayerJump class.
        public enum JumpType { SinglePressToJump, HoldToJumpHigher };
        
        // Public variables that shouldn't be shown in the inspector.
        [HideInInspector]
        public bool jump = false;                               // Condition for whether the player should jump.
        [HideInInspector]
        public int jumps;                                       // Count how many jumps are currently allowed.
        [HideInInspector]
        public bool walkingOnJump;                              // Remember if the player was walking or running when the jump was initialized.
        
        // Public variables.
        [System.Serializable]
        public class SinglePressToJump {
            [Tooltip("Set the player's jump force.")]
            public float jumpForce = 400f;
            [Tooltip("Set the player's jump force when performing a double jump.")]
            public float doubleJumpForce = 300f;
        }
        
        [System.Serializable]
        public class HoldToJumpHigher {
            [Tooltip("Set the initial jump's Y velocity.")]
            public float initialJump = 5f;
            [Tooltip("Set the initial double jump's Y velocity.")]
            public float initialDoubleJump = 5f;
            [Tooltip("Set the duration of the player's jump.")]
            public float jumpTime = 0.3f;
            [Tooltip("Set the jump's force while holding down the jump button.")]
            public float jumpForce = 20f;
            [Tooltip("Set the double jump's force while holding down the jump button.")]
            public float doubleJumpForce = 15f;
        }
        
        [System.Serializable]
        public class DoubleJumping {
            [Tooltip("Set the total amount of jumps that can be performed in sequence. Setting this to '2' will allow a simple double jump.")]
            public int totalJumps = 2;
            [Tooltip("This allows you to set a window for double jumping. Disabling this variable will allow you to double jump at any time while in the air. Enabling this will only allow the double jump while in the window.")]
            public bool jumpWindow = true;
            [Tooltip("When Jump Window is enabled, you can set the jump window's minimum velocity. Setting this lower creates a larger jump window.")]
            public float jumpWindowMin = -4f;
            [Tooltip("When Jump Window is enabled, you can set the jump window's maximum velocity. Setting this higher creates a larger jump window.")]
            public float jumpWindowMax = 4f;
        }
        
        [System.Serializable]
        public class AirMovement {
            [Tooltip("This allows you to control the player's change in movement while in the air. Setting this to a lower value will make changing direction in the air a lot more difficult.")]
            public float changeFactor = 1.0f;
            [Tooltip("This allows you to control the player's speed while in the air. This allows you to make air movement slower or faster than movement on the ground.")]
            public float speedFactor = 1.0f;
            [Tooltip("By enabling this variable, the player can change between walking and running while in the air. When you disable this variable, the player isn't allowed to change between those two states.")]
            public bool walkAndRun = true;
            [Tooltip("This allows you to reset if the player is walking or running while interacting with a wall. So when you jump on the wall while walking, you can wall jump in a running state.")]
            public bool resetOnWall = true;
        }
        
        [Tooltip("Select if the player should jump as high as he can when pressing the jump button (Single Press To Jump) or that it depends on how long you hold down the jump button (Hold To Jump Higher).")]
        public JumpType jumpType = JumpType.HoldToJumpHigher;
        [Tooltip("These variables are used when Jump Type is set to Single Press To Jump.")]
        public SinglePressToJump singlePressToJump;
        [Tooltip("These variables are used when Jump Type is set to Hold To Jump Higher.")]
        public HoldToJumpHigher holdToJumpHigher;
        [Tooltip("All variables used for double jumping are located here.")]
        public DoubleJumping doubleJumping;
        [Tooltip("All variables used for movement in the air are located here.")]
        public AirMovement airMovement;
        
        //---------------------------------------------------
        // Custom Code
        public bool isPaused = false;
        public CustomPlayerActions customPlayerActions;
        public List<CustomEventsManager> onJumpEvents = null;
        //---------------------------------------------------
        
        // Private variables.
        private Player player;                                  // Get the Player class.
        private bool doubleJump = false;                        // Check if the player is performing a double jump.
        private bool initialJump = false;                       // Used for holdToJumpHigher to make the player perform an initial jump.
        private float jumpTimer;                                // Used for the holdToJumpHigher jumps. Determines how long the player can jump.
        
        //----------------------
        // Custom Code
        protected void Awake() {
            Initialize();
        }
        //----------------------
        
        // Use this for initialization.
        protected void Start() {
            // Setting up references.
            player = this.gameObject.GetComponent<Player>();
        }
        
        // Update is called once per frame.
        protected void Update() {
            if(!isPaused) {
                // Reset total jumps allowed when not performing a jump and grounded or when on a moving/sinking platform.
                if(!jump
                    && (player.grounded || player.IsStuckToPlatform())
                    && ((Mathf.Round(player.GetComponent<Rigidbody2D>().velocity.y) == 0) || ((player.OnMovingPlatform() || player.OnSinkingPlatform()) && player.GetComponent<Rigidbody2D>().velocity.y == player.GetPlatform().GetComponent<Rigidbody2D>().velocity.y))) {
                    jumps = doubleJumping.totalJumps;
                }
                
                // If the jump button is pressed, jumps are allowed and the player is not dashing, sliding, on a ladder or crouching under an obstacle...
                if(!jump
                    && customPlayerActions.GetCustomPlayerActionSet().Jump.WasPressed
                    // && jumps > 0         // Remove 2015/10/20 - Fixes slope jump bug....
                    && !player.dashing
                    && !player.sliding
                    && !player.onLadder
                    && (!player.crouching || (player.crouching && player.AllowedToStandUp()))) {
                    // If the player is grounded...
                    if(player.grounded) {
                        // ... initialize jump.
                        InitJump();
                        // If the player is not grounded and totalJumps is higher than 1...
                    }
                    else if(doubleJumping.totalJumps > 1) {
                        // ... initialize jump if the Y velocity is inside the double jump window (or when there isn't a window).
                        if(!doubleJumping.jumpWindow
                            || (doubleJumping.jumpWindow && player.GetComponent<Rigidbody2D>().velocity.y > doubleJumping.jumpWindowMin && player.GetComponent<Rigidbody2D>().velocity.y < doubleJumping.jumpWindowMax)) {
                            doubleJump = true;
                            InitJump();
                        }
                    }
                }
            }
        }
        
        // This function is called every fixed framerate frame.
        void FixedUpdate() {
            // If the player should jump...
            if(jump) {
                // If the player is jumping down a platform and is doing the first jump...
                if(player.jumpDown
                    && jumps == doubleJumping.totalJumps) {
                    // ... add a small Y force.
                    player.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 200f));
                    
                    // If the player is on a platform...
                    if(player.OnPlatform()) {
                        // Set the moving platform to null.
                        player.UnstickFromPlatform();
                    }
                    
                    // Reset the jumping variables.
                    ResetJumpVars();
                    
                    // Reset jumpDown.
                    player.jumpDown = false;
                    
                    // Stop the player from jumping.
                    return;
                }
                
                // If this is the initial jump...
                if(initialJump) {
                    // Check if the player is running when the jump is being performed.
                    walkingOnJump = player.walking;
                    
                    // When double jumping, set Y velocity to 0 to make sure the jump force is applied correctly.
                    if(doubleJump) {
                        player.SetYVelocity(0);
                    }
                    
                    // Decrease total jumps allowed with 1.
                    jumps--;
                    
                    // If the player is on a moving platform and should not keep the speed when jumping from a moving platform...
                    if(player.OnMovingPlatform() && !player.KeepSpeedOnJump()) {
                        // Set the X velocity to 0.
                        player.SetXVelocity(0);
                    }
                }
                
                // Get the jump factor.
                float jumpFactor = player.GetJumpFactor();
                // If you need to hold the Jump input to jump higher...
                if(jumpType == JumpType.HoldToJumpHigher) {
                    // When there is an initial jump...
                    if(initialJump) {
                        // ... set the y velocity to the player's initial jump value.
                        float yVel = jumpFactor * (doubleJump ? holdToJumpHigher.initialDoubleJump : holdToJumpHigher.initialJump);
                        
                        // If the player is on a moving platform...
                        if(player.OnMovingPlatform()) {
                            // ... get the current platform.
                            GameObject platform = player.GetPlatform();
                            Rigidbody2D platformRigidbody = platform.GetComponent<Rigidbody2D>();
                            // If the platform's Y velocity is greater than 0...
                            if(platformRigidbody.velocity.y > 0) {
                                // ... make sure the y velocity of this platform is taken into account when jumping.
                                yVel += platformRigidbody.velocity.y;
                            }
                        }
                        
                        // If the player is on a platform...
                        if(player.OnPlatform()) {
                            // Set the moving platform to null.
                            player.UnstickFromPlatform();
                        }
                        
                        // Make sure the player's velocity is set.
                        player.SetYVelocity(yVel);
                        
                        // Set initialJump to false.
                        initialJump = false;
                        // When the jump button is being pressed and the timer isn't finished yet...
                    }
                    else if(customPlayerActions.GetCustomPlayerActionSet().Jump.IsPressed
                            && jumpTimer > 0) {
                        // ... decrease the timer's value.
                        jumpTimer -= Time.deltaTime;
                        
                        // Set the Y Force for the player.
                        player.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpFactor * (doubleJump ? holdToJumpHigher.doubleJumpForce : holdToJumpHigher.jumpForce)));
                        // When the timer is finished or the jump button isn't being pressed...
                    }
                    else {
                        // ... reset the jumping variables.
                        ResetJumpVars();
                    }
                    // Or else if you need a single press to perform a jump...
                }
                else {
                    // Add a vertical force to the player.
                    player.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpFactor * (doubleJump ? singlePressToJump.doubleJumpForce : singlePressToJump.jumpForce)));
                    
                    // If the player is on a platform...
                    if(player.OnPlatform()) {
                        // Set the moving platform to null.
                        player.UnstickFromPlatform();
                    }
                    
                    // Reset the jumping variables.
                    ResetJumpVars();
                }
            }
        }
        
        //----------------------
        // Custom Code
        protected void Initialize() {
            // Custom Player Actions
            if(customPlayerActions == null) {
                this.gameObject.LogComponentError("customPlayerActions", this.GetType());
            }
            // On Jump Events
            if(onJumpEvents == null) {
                onJumpEvents = new List<CustomEventsManager>();
            }
            foreach(CustomEventsManager customEventsManager in onJumpEvents) {
                if(customEventsManager == null) {
                    Debug.LogError(this.gameObject.transform.GetFullPath() + " has a NULL element in its onJumpEvents. Please fix this issue.");
                }
            }
        }
        //----------------------
        
        // Reset the jumping variables.
        void ResetJumpVars() {
            // Make sure the player can't jump again until the jump conditions from Update are satisfied.
            jump = false;
            
            // Set initialJump to false.
            initialJump = false;
            
            // Reset the double jump variable.
            doubleJump = false;
            
            // Reset the jumpTimer.
            jumpTimer = holdToJumpHigher.jumpTime;
        }
        
        // Initialize the jump.
        public void InitJump() {
            // Set jump and initialJump to true.
            jump = true;
            initialJump = true;
            player.jumping = true;
            
            // Reset the jumpTimer.
            jumpTimer = holdToJumpHigher.jumpTime;
            
            //----------------------
            // Custom Code
            FireJumpEvents();
            //----------------------
        }
        
        //----------------------
        // Custom Code
        protected void FireJumpEvents() {
            foreach(CustomEventsManager customEventsManager in onJumpEvents) {
                customEventsManager.Execute();
            }
        }
        
        public void Pause() {
            isPaused = true;
        }
        
        public void Unpause() {
            isPaused = false;
        }
        //----------------------
    }
}