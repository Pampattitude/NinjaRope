using UnityEngine;
using System.Collections;

public class PermanentRopePlayerController: MonoBehaviour {
    private float   speed_ = 104f, ropeSpeed_ = 5f;
    private float   minRopeLength_ = 1f, maxRopeLength_ = 8f;

	void    FixedUpdate() {
        NinjaRope   rope = FindObjectOfType<NinjaRope>();

        Vector3     upVector = (rope.lastAnchor.anchor - gameObject.transform.position).normalized,
                    rightVector = (Quaternion.Euler(0f, 0f, -90f) * upVector).normalized;

        Vector3 posDelta = Vector3.zero;

	    if (Input.GetKey(KeyCode.W))
            rope.ropeLength -= this.ropeSpeed_ * Time.fixedDeltaTime;
            //posDelta += upVector * Time.fixedDeltaTime * this.speed_;
	    if (Input.GetKey(KeyCode.S))
            rope.ropeLength += this.ropeSpeed_ * Time.fixedDeltaTime;
            //posDelta -= upVector * Time.fixedDeltaTime * this.speed_;

        if (this.minRopeLength_ > rope.ropeLength)
            rope.ropeLength = this.minRopeLength_;
        if (this.maxRopeLength_ < rope.ropeLength)
            rope.ropeLength = this.maxRopeLength_;

    	if (Input.GetKey(KeyCode.A))
            posDelta -= rightVector * this.speed_ * Time.fixedDeltaTime;
	    if (Input.GetKey(KeyCode.D))
            posDelta += rightVector * this.speed_ * Time.fixedDeltaTime;

        if (0.001f < posDelta.magnitude)
            gameObject.GetComponent<Rigidbody2D>().AddForce(posDelta);
	}
}
