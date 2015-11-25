using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class NinjaRopePlayerController: MonoBehaviour {
    private float       speed_ = 120f, ropeSpeed_ = 5f;
    private float       minRopeLength_ = 0.1f, maxRopeLength_ = 12f;
    private float       ropeAngle_ = 20f;

    public GameObject   hookPrefab;
    private GameObject  hookInstance_;
    private NinjaHook   hookScript_;
    private bool        isHooking_ = false;

    public GameObject   ropePrefab;
    private GameObject  ropeInstance_;
    private NinjaRope   ropeScript_;

    private Rigidbody2D rigidbody_;

    void    Start() {
        this.rigidbody_ = gameObject.GetComponent<Rigidbody2D>();
    }

    void    Update() {
        if (!this.isHooking_) {
            // No hook launched / hooked, so can launch it
            if (Input.GetKeyDown(KeyCode.A))
                this.launchHook_(Quaternion.Euler(0f, 0f,  this.ropeAngle_) * Vector3.up);
            if (Input.GetKeyDown(KeyCode.D))
                this.launchHook_(Quaternion.Euler(0f, 0f, -this.ropeAngle_) * Vector3.up);
        }
        else {
            // Hook is either launched or hooked
            // No input, remove the hook
            if (Input.GetKeyUp(KeyCode.A) ||
                Input.GetKeyUp(KeyCode.D)) {
                this.removeHook_();
                return ;
            }

            if (!this.hookScript_.isHooked()) {
                // The hook is launched, but not yet hooked
                if (this.maxRopeLength_ < Vector3.Distance(this.hookInstance_.transform.position, gameObject.transform.position)) // Remove hook if it took too long to find a spot to hook on
                    this.removeHook_();
            }
            else {
                // The hook is hooked on a surface
                Vector3     upVector = (this.ropeScript_.lastAnchor.anchor - gameObject.transform.position).normalized,
                            rightVector = (Quaternion.Euler(0f, 0f, -90f) * upVector).normalized;
                Vector3 posDelta = Vector3.zero;

                this.hookScript_.rotateTo(this.ropeScript_.postFirstAnchor.anchor);

                // Move the player
    	        if (Input.GetKey(KeyCode.A))
                    posDelta -= rightVector * this.speed_ * Time.fixedDeltaTime;
	            if (Input.GetKey(KeyCode.D))
                    posDelta += rightVector * this.speed_ * Time.fixedDeltaTime;

                if (0.0001f < posDelta.magnitude)
                    this.rigidbody_.AddForce(posDelta);

                // Move up or down the rope
	            if (Input.GetKey(KeyCode.W))
                    this.ropeScript_.ropeLength -= this.ropeSpeed_ * Time.fixedDeltaTime;
	            if (Input.GetKey(KeyCode.S))
                    this.ropeScript_.ropeLength += this.ropeSpeed_ * Time.fixedDeltaTime;

                if (this.minRopeLength_ > this.ropeScript_.ropeLength)
                    this.ropeScript_.ropeLength = this.minRopeLength_;
                if (this.maxRopeLength_ < this.ropeScript_.ropeLength)
                    this.ropeScript_.ropeLength = this.maxRopeLength_;
            }
        }
    }

    private void    launchHook_(Vector3 spd) {
        // Instantiate hook, and make the player the hook parent to ensure the hook position is affected by the player (TMP: DOES NOT WORK)
        this.hookInstance_ = Object.Instantiate(this.hookPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        this.hookScript_ = this.hookInstance_.GetComponent<NinjaHook>();
        this.hookScript_.direction = spd;

        this.ropeInstance_ = Object.Instantiate<GameObject>(this.ropePrefab);
        this.ropeScript_ = this.ropeInstance_.GetComponent<NinjaRope>();
        this.ropeScript_.baseEntity = this.hookInstance_;
        this.ropeScript_.endEntity = gameObject;
        this.ropeScript_.removeLength(); // Remove the max length of the rope

        this.hookScript_.onHooked += this.onHooked;

        this.isHooking_ = true;
    }

    private void    removeHook_() {
        // Destroy both the hook and the rope (duh!)
        Destroy(this.ropeInstance_);
        Destroy(this.hookInstance_);

        this.isHooking_ = false;
    }

    public void     onHooked() {
        // Reset the max length of the rope
        this.ropeScript_.setLength(this.ropeScript_.getRopeLength());
    }
}
