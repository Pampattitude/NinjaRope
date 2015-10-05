using UnityEngine;
using System.Collections;

public class NinjaRopePlayerController: MonoBehaviour {
    private float   speed_ = 120f, ropeSpeed_ = 5f;
    private float   minRopeLength_ = 0.1f, maxRopeLength_ = 12f;
    private float   ropeAngle_ = 20f;

    public GameObject   hookPrefab;
    private GameObject  hook_;
    private bool        isHooking_ = false;

    public GameObject   ropePrefab;
    private GameObject  rope_;

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
            NinjaRope   rope = this.rope_.GetComponent<NinjaRope>();
        
            // No input, remove the hook
            if (Input.GetKeyUp(KeyCode.A) ||
                Input.GetKeyUp(KeyCode.D)) {
                this.removeHook_();
                return ;
            }

            if (!this.hook_.GetComponent<NinjaHook>().isHooked()) {
                // The hook is launched, but not yet hooked
                if (this.maxRopeLength_ < Vector3.Distance(this.hook_.transform.position, gameObject.transform.position)) // Remove hook if it took too long to find a spot to hook on
                    this.removeHook_();
            }
            else {
                // The hook is hooked on a surface
                Vector3     upVector = (rope.lastAnchor.anchor - gameObject.transform.position).normalized,
                            rightVector = (Quaternion.Euler(0f, 0f, -90f) * upVector).normalized;
                Vector3 posDelta = Vector3.zero;

                // Move the player
    	        if (Input.GetKey(KeyCode.A))
                    posDelta -= rightVector * this.speed_ * Time.fixedDeltaTime;
	            if (Input.GetKey(KeyCode.D))
                    posDelta += rightVector * this.speed_ * Time.fixedDeltaTime;

                if (0.0001f < posDelta.magnitude)
                    gameObject.GetComponent<Rigidbody2D>().AddForce(posDelta);

                // Move up or down the rope
	            if (Input.GetKey(KeyCode.W))
                    rope.ropeLength -= this.ropeSpeed_ * Time.fixedDeltaTime;
	            if (Input.GetKey(KeyCode.S))
                    rope.ropeLength += this.ropeSpeed_ * Time.fixedDeltaTime;

                if (this.minRopeLength_ > rope.ropeLength)
                    rope.ropeLength = this.minRopeLength_;
                if (this.maxRopeLength_ < rope.ropeLength)
                    rope.ropeLength = this.maxRopeLength_;
            }
        }
    }

    private void    launchHook_(Vector3 spd) {
        // Instantiate hook, and make the player the hook parent to ensure the hook position is affected by the player (TMP: DOES NOT WORK)
        this.hook_ = Object.Instantiate(this.hookPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        this.hook_.GetComponent<NinjaHook>().direction = spd;

        this.rope_ = Object.Instantiate<GameObject>(this.ropePrefab);
        this.rope_.GetComponent<NinjaRope>().baseEntity = this.hook_;
        this.rope_.GetComponent<NinjaRope>().endEntity = gameObject;
        this.rope_.GetComponent<NinjaRope>().removeLength(); // Remove the max length of the rope

        this.hook_.GetComponent<NinjaHook>().onHooked += this.onHooked;

        this.isHooking_ = true;
    }

    private void    removeHook_() {
        // Destroy both the hook and the rope (duh!)
        Destroy(this.hook_);
        Destroy(this.rope_);

        this.isHooking_ = false;
    }

    public void     onHooked() {
        // Reset the max length of the rope
        this.rope_.GetComponent<NinjaRope>().setLength(this.rope_.GetComponent<NinjaRope>().getRopeLength() + gameObject.GetComponent<Rigidbody2D>().velocity.magnitude * Time.deltaTime);
    }
}
