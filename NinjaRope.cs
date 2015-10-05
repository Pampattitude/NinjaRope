using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class NinjaRope: MonoBehaviour {
    private Rigidbody2D         rigidbody_;
    private LineRenderer        lineRenderer_;
    private BoxCollider2D       collider_;

    [System.Serializable]
    public class RopeAnchor {
        public Vector3  anchor; // The position of the anchor
        public float    side; // The side of the perpendicular comparison

        public RopeAnchor(Vector3 a, float s) {
            this.anchor = a;
            this.side = s;
        }
    }

    private List<RopeAnchor>    anchors_ = new List<RopeAnchor>();

    public GameObject           baseEntity;
    public GameObject           endEntity; // The player

    public float                ropeWidth = 0.12f; // The width of the rope
    public float                ropeLength = 3f; // The length
    private float               minDistanceBetweenPoints_ = 0.1f;

    protected void      Start() {
        this.initRigidbody_();
        this.initCollider_();

        this.lineRenderer_ = gameObject.GetComponent<LineRenderer>();
        this.lineRenderer_.SetWidth(this.ropeWidth, this.ropeWidth);

        this.anchors_.Add(new RopeAnchor(this.baseEntity.transform.position, 0f));
        this.anchors_.Add(new RopeAnchor(this.endEntity.transform.position, 0f));

        this.updateJoint_();
    }
	
	protected void      FixedUpdate () {
	    this.baseAnchor = new RopeAnchor(this.baseEntity.transform.position, 0f);
	    this.targetAnchor = new RopeAnchor(this.endEntity.transform.position, 0f);

        this.popAnchors_();
        this.updateCollider_();
        this.updateRendering_();
        this.updateJoint_();
	}


    // Initialization
    private void    initRigidbody_() {
        this.rigidbody_ = gameObject.GetComponent<Rigidbody2D>();
        if (!this.rigidbody_)
            this.rigidbody_ = gameObject.AddComponent<Rigidbody2D>();

        this.rigidbody_.mass = float.MaxValue;

        this.rigidbody_.interpolation = RigidbodyInterpolation2D.None;
        this.rigidbody_.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        this.rigidbody_.sleepMode = RigidbodySleepMode2D.StartAwake;

        this.rigidbody_.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void    initCollider_() {
        this.collider_ = gameObject.GetComponent<BoxCollider2D>();
        if (!this.collider_)
            this.collider_ = gameObject.AddComponent<BoxCollider2D>();

        this.collider_.isTrigger = false;

        this.collider_.offset = Vector2.zero;
        this.collider_.size = new Vector2(this.ropeWidth / 2f, 0f);
    }
    // !Initialization


    // Update
    private void    popAnchors_() {
        bool        goOn = true;

        while (2 < this.anchors_.Count && true == goOn) {
            goOn = false;

            // Calculate the dot product of the vector perpendicular to the anchor
            // and the actual segment vector. If it's greater than zero, it means the
            // they are kind of facing the same way, so the anchor can be safely removed
            Vector3     prev = this.lastAnchor.anchor - this.preLastAnchor.anchor,
                        act = this.targetAnchor.anchor - this.lastAnchor.anchor;
            Vector3     prevPerpendicular = Quaternion.Euler(0f, 0f, 90f * this.lastAnchor.side) * prev;

            if (0f <= Vector3.Dot(prevPerpendicular, act)) {
                this.lastAnchor = this.targetAnchor;
                this.anchors_.RemoveAt(this.anchors_.Count - 1);

                goOn = true;
            }
        }

        goOn = true;
        while (2 < this.anchors_.Count && true == goOn) {
            // Same goes from the beginning of the rope, because the base of the rope (the hook) could be moving
            goOn = false;

            Vector3     prev = this.postFirstAnchor.anchor - this.baseAnchor.anchor,
                        act = this.postPostFirstAnchor.anchor - this.postFirstAnchor.anchor;
            Vector3     prevPerpendicular = Quaternion.Euler(0f, 0f, 90f * this.postFirstAnchor.side) * prev;

            if (0f <= Vector3.Dot(prevPerpendicular, act)) {
                this.postFirstAnchor = this.postPostFirstAnchor;
                this.anchors_.RemoveAt(2);

                goOn = true;
            }
        }
    }

    private void    updateCollider_(bool direct = true) {
        this.collider_.size = new Vector2(this.ropeWidth / 2f, Vector2.Distance(this.lastAnchor.anchor, this.targetAnchor.anchor));
        gameObject.transform.position = (this.lastAnchor.anchor + this.targetAnchor.anchor) / 2f;
        gameObject.transform.eulerAngles = new Vector3(0f, 0f, NinjaRope.pointsToAngle(this.lastAnchor.anchor, this.targetAnchor.anchor));
    }

    private void    updateRendering_() {
        this.lineRenderer_.SetVertexCount(this.anchors_.Count);

        for (int i = 0 ; this.anchors_.Count > i ; ++i)
            this.lineRenderer_.SetPosition(i, this.anchors_[i].anchor);
    }

    private void    updateJoint_() {
        DistanceJoint2D joint = gameObject.GetComponent<DistanceJoint2D>();
        if (!joint) {
            joint = gameObject.AddComponent<DistanceJoint2D>();

            if (float.MaxValue - 0.001f > this.ropeLength)
                joint.maxDistanceOnly = false;
            else
                joint.maxDistanceOnly = true;
        }
        if (float.MaxValue - 0.001f > this.ropeLength)
            joint.maxDistanceOnly = false;
        else
            joint.maxDistanceOnly = true;

        joint.anchor = gameObject.transform.InverseTransformPoint(this.lastAnchor.anchor);
        joint.connectedBody = this.endEntity.GetComponent<Rigidbody2D>();
        joint.connectedAnchor = this.endEntity.transform.InverseTransformPoint(this.targetAnchor.anchor);

        float   distance = this.ropeLength - this.getActualRopeLength_();
        if (0f > distance)
            distance = 0f;
        joint.distance = distance;
    }

    private void    addAnchor_(RopeAnchor anc) {
        this.anchors_.Insert(this.anchors_.Count - 1, anc);
    }
    // !Update


    // Listeners
    private class CollisionPointDistanceFromSort { // Sort functor (not really a functor, actually)
        public Vector3  basePos;
        public CollisionPointDistanceFromSort(Vector3 v) { this.basePos = v; }
        public int      compare(ContactPoint2D c1, ContactPoint2D c2) {
            float       diff = Vector3.Distance(this.basePos, c1.point) - Vector3.Distance(this.basePos, c2.point);
            if (0 > diff) return -1; else if (0 < diff) return 1; return 0;
        }
    }
    protected void  OnCollisionStay2D(Collision2D c) {
        // We get the nearest vertex of the ground
        System.Array.Sort(c.contacts, (new CollisionPointDistanceFromSort(this.lastAnchor.anchor)).compare);
        foreach (ContactPoint2D con in c.contacts) {
            Vector3     nearestVertex = NinjaRope.nearestVertexTo(c.gameObject, con.point);

            // Then, we make sure we're not too close to the previous point (else, since this is "on stay", new points would spawn like hell)
            Vector3 padd = nearestVertex - c.gameObject.transform.position;
            padd.Normalize();
            Vector3 finalNearestVertex = nearestVertex + padd * this.ropeWidth / 2f;

            if (this.minDistanceBetweenPoints_ < Vector3.Distance(this.lastAnchor.anchor, finalNearestVertex)) {
                // We get the angle between the latest segment and the anchor-middle of platform
                float   angle = Mathf.DeltaAngle(NinjaRope.pointsToAngle(nearestVertex - this.lastAnchor.anchor, nearestVertex - c.gameObject.transform.position), NinjaRope.pointsToAngle(nearestVertex, this.lastAnchor.anchor));
                float   side = 0f >= angle ? -1f : 1f;

                // Finally, we add the anchor to the list
                this.addAnchor_(new RopeAnchor(finalNearestVertex, side));
            }
        }
    }
    // !Listeners


    // Utils
    private static float    pointsToAngle(Vector2 anc1, Vector2 anc2) {
        // Returns the angle between both vectors; note that the angle is NOT between -180 and 180
        return (Mathf.Atan2(anc2.y - anc1.y, anc2.x - anc1.x) * Mathf.Rad2Deg - 90f);
    }

    private static Vector3  nearestVertexTo(GameObject go, Vector3 point, float z = 0f) {
        // Get the nearest vertex, on a polygon collider, to the `point` given
        Vector3     act = new Vector3(1000f, 1000f, 1000f);
        bool        found = false;

        foreach (Vector3 v in go.GetComponent<PolygonCollider2D>().points) {
            if (Vector3.Distance(point, act) > Vector3.Distance(point, go.transform.TransformPoint(v))) {
                act = go.transform.TransformPoint(v); act.z = 0f;
                found = true;
            }
        }

        if (!found)
            return point;
        return new Vector3(act.x, act.y, z);
    }

    
    public RopeAnchor baseAnchor    { get { return this.anchors_[0]; }                          set { this.anchors_[0] = value; } }
    public RopeAnchor postFirstAnchor { get { return this.anchors_[1]; }                        set { this.anchors_[1] = value; } }
    public RopeAnchor postPostFirstAnchor { get { return this.anchors_[2]; }                        set { this.anchors_[2] = value; } }
    public RopeAnchor targetAnchor  { get { return this.anchors_[this.anchors_.Count - 1]; }    set { this.anchors_[this.anchors_.Count - 1] = value; } }
    public RopeAnchor lastAnchor    { get { return this.anchors_[this.anchors_.Count - 2]; }    set { this.anchors_[this.anchors_.Count - 2] = value; } }
    public RopeAnchor preLastAnchor { get { return this.anchors_[this.anchors_.Count - 3]; }    set { this.anchors_[this.anchors_.Count - 3] = value; } }

    private float   getActualRopeLength_() {
        float dist = 0f;
        for (int i = 0 ; this.anchors_.Count - 1 > i + 1 ; ++i) // Note: -1 because we don't want to account for the target
            dist += Vector3.Distance(this.anchors_[i].anchor, this.anchors_[i + 1].anchor);
        return dist;
    }
    // Different from the previous one because it also accounts for the last chunk, ending with the target
    public float    getRopeLength() {
        float dist = 0f;
        for (int i = 0 ; this.anchors_.Count > i + 1 ; ++i)
            dist += Vector3.Distance(this.anchors_[i].anchor, this.anchors_[i + 1].anchor);
        return dist;
    }

    public void     setLength(float len) {
        this.ropeLength = len;

        DistanceJoint2D joint = gameObject.GetComponent<DistanceJoint2D>();
        if (joint)
            joint.maxDistanceOnly = false;
    }
    public void     removeLength() {
        this.ropeLength = float.MaxValue;

        DistanceJoint2D joint = gameObject.GetComponent<DistanceJoint2D>();
        if (joint)
            joint.maxDistanceOnly = true;
    }
    // !Utils
}
