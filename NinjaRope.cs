#define DEBUG_NINJA_ROPE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class NinjaRope: MonoBehaviour {
    private static int          criticalIterationCount = 1000; // Debug, useful with DEBUG_NINJA_ROPE

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

    public float                raycastStep = 10f;

    public LayerMask            collisionMask;

    protected void      Start() {
        this.initRigidbody_();

        this.lineRenderer_ = gameObject.GetComponent<LineRenderer>();
        this.lineRenderer_.SetWidth(this.ropeWidth, this.ropeWidth);

        this.anchors_.Add(new RopeAnchor(this.baseEntity.transform.position, 0f));
        this.anchors_.Add(new RopeAnchor(this.endEntity.transform.position, 0f));

        this.updateJoint_();
    }
	
	protected void      Update() {
	    this.baseAnchor = new RopeAnchor(this.baseEntity.transform.position, 0f);
	    this.targetAnchor = new RopeAnchor(this.endEntity.transform.position, 0f);

        this.popAnchors_();
        this.updateRaycastCollision_();
        this.updateJoint_();
        this.updateRendering_();
	}


#if DEBUG_NINJA_ROPE
    void    OnDrawGizmos() {
        float   opacity = 2f / 3f;
        Color   baseColor = Gizmos.color;

        Gizmos.color = new Color(0f, 0.5f, 1f, opacity);
        for (int i = 1 ; this.anchors_.Count - 1 > i ; ++i) {
            Gizmos.DrawSphere(this.anchors_[i].anchor, this.ropeWidth);
        }

        Gizmos.color = new Color(1f, 0.5f, 0f, opacity);
        Gizmos.DrawSphere(this.baseAnchor.anchor, this.ropeWidth);
        Gizmos.color = new Color(1f, 0f, 0.5f, opacity);
        Gizmos.DrawSphere(this.targetAnchor.anchor, this.ropeWidth);

        Gizmos.color = baseColor;
    }
#endif

    // Initialization
    private void    initRigidbody_() {
        // Rigidbody is necessary for the joint collision to work
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
    private void    updateRaycastCollision_() {
        int         loopCount = 0,
                    realLoopCount = 0;
        Vector3     prevPos = this.lastAnchor.anchor;

        do {
            Vector3     linecastTarget = prevPos + (this.targetAnchor.anchor - this.lastAnchor.anchor) / this.raycastStep;

            // Check that the linecast is still being done in bounds of the last anchor -> target anchor vector
            if (0f >= Vector3.Dot(this.targetAnchor.anchor - prevPos, this.targetAnchor.anchor - linecastTarget))
                linecastTarget = this.targetAnchor.anchor;

            RaycastHit2D    rch = Physics2D.Linecast(prevPos, linecastTarget, this.collisionMask);
            if (!rch || 0.0001f >= Vector3.Distance(prevPos, rch.point))
                rch = Physics2D.Linecast(linecastTarget, prevPos, this.collisionMask);

            if (rch && 0.0001f < Vector3.Distance(prevPos, rch.point)) {
                Vector3     point = rch.point;

                // We get the angle between the latest segment and the anchor-middle of platform
                float   angle = Mathf.DeltaAngle(NinjaRope.pointsToAngle(point - this.lastAnchor.anchor, point - rch.collider.gameObject.transform.position), NinjaRope.pointsToAngle(point, this.lastAnchor.anchor));
                float   side = 0f >= angle ? -1f : 1f;

                // Finally, we add the anchor to the list
                this.addAnchor_(new RopeAnchor(point, side));

                prevPos = point;
                loopCount = 0;
            }
            else {
                prevPos += (this.targetAnchor.anchor - this.lastAnchor.anchor) / this.raycastStep; // We try to move the previous position forward a bit
                // Note that it means the collision detection gets less and less accurate the further we get to the target anchor
                ++loopCount;
            }

            ++realLoopCount;
        } while (Mathf.Sign(this.lastAnchor.anchor.x - this.targetAnchor.anchor.x) == Mathf.Sign(prevPos.x - this.targetAnchor.anchor.x) &&
                 Mathf.Sign(this.lastAnchor.anchor.y - this.targetAnchor.anchor.y) == Mathf.Sign(prevPos.y - this.targetAnchor.anchor.y)
#if DEBUG_NINJA_ROPE
                 && NinjaRope.criticalIterationCount > realLoopCount
#endif
        );

#if DEBUG_NINJA_ROPE
        if (NinjaRope.criticalIterationCount == realLoopCount)
            Debug.Log("NinjaRope: Critical iteration count (" + NinjaRope.criticalIterationCount + ", loop count was " + loopCount + ") attained, please contact the creator of the script!");
#endif
    }

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

            if (0f < Vector3.Dot(prevPerpendicular, act)) {
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


    // Utils
    private static float    pointsToAngle(Vector2 anc1, Vector2 anc2) {
        // Returns the angle between both vectors; note that the angle is NOT between -180 and 180
        return (Mathf.Atan2(anc2.y - anc1.y, anc2.x - anc1.x) * Mathf.Rad2Deg - 90f);
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
