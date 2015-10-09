using UnityEngine;
using System.Collections;

public class NinjaHook: MonoBehaviour {
    public Vector3      direction;
    public float        speed;

    private bool        isHooked_ = false;
    public delegate void     HookAction();
    public event HookAction  onHooked;

    private Rigidbody2D rigidbody_;

	protected void  Start() {
        this.initRigidbody_();

        // Normalize direction and adapt hook angle
	    this.direction.Normalize();
        gameObject.transform.eulerAngles = new Vector3(0f, 0f, NinjaHook.pointsToAngle(Vector3.zero, this.direction));
	}
	
	protected void  FixedUpdate() {
        // When hooked, the hook stops moving
        if (this.isHooked_)
            return ;

        // Make sure the rotation is right (because of the parent)...
        gameObject.transform.eulerAngles = new Vector3(0f, 0f, NinjaHook.pointsToAngle(Vector3.zero, this.direction));
        // ... and also make sure the velocity hasn't changed
	    this.rigidbody_.velocity = this.direction * this.speed;
	}


    // Initialization
    private void    initRigidbody_() {
        this.rigidbody_ = gameObject.GetComponent<Rigidbody2D>();
        if (!this.rigidbody_) {
            this.rigidbody_ = gameObject.AddComponent<Rigidbody2D>();

            // No mass, no gravity. The entity is entirely managed by script
            this.rigidbody_.mass = 0f;
            this.rigidbody_.gravityScale = 0f;
        }

        this.rigidbody_.interpolation = RigidbodyInterpolation2D.Interpolate;
        this.rigidbody_.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        this.rigidbody_.sleepMode = RigidbodySleepMode2D.StartAwake;
    }
    // !Initialization

   
    // Listeners
    protected void  OnCollisionEnter2D(Collision2D c) {
        // Make kinematic to ensure the hook won't move anymore
        this.rigidbody_.isKinematic = true;

        gameObject.transform.eulerAngles = new Vector3(0f, 0f, NinjaHook.pointsToAngle(Vector3.zero, this.direction));

        this.isHooked_ = true;
        if (null != this.onHooked)
            this.onHooked();
    }
    protected void  OnTriggerEnter2D(Collider2D c) {
        // Make kinematic to ensure the hook won't move anymore
        this.rigidbody_.isKinematic = true;

        gameObject.transform.eulerAngles = new Vector3(0f, 0f, NinjaHook.pointsToAngle(Vector3.zero, this.direction));

        this.isHooked_ = true;
        if (null != this.onHooked)
            this.onHooked();
    }
    // !Listeners


    // Util
    public bool     isHooked() { return this.isHooked_; }

    private static float    pointsToAngle(Vector2 anc1, Vector2 anc2) {
        // Returns the angle between both vectors; note that the angle is NOT between -180 and 180
        return (Mathf.Atan2(anc2.y - anc1.y, anc2.x - anc1.x) * Mathf.Rad2Deg - 90f);
    }
    // !Util
}
