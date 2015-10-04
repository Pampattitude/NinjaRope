# NinjaRope
Unity3D implementation of the Ninja Rope in Worms Armaggedon.

This implementation has been made for my game, [Scufflers](http://gamejolt.com/games/scufflers-pre-alpha/92083), currently in pre-alpha.

To use, just create a gameobject with a LineRenderer (for the rope rendering) and the script (duh!).
Included is an example player controller to test it out.

C&C and PRs are welcome!

- - - -

## Known bugs

* ONLY works with `PolygonCollider2D`. PRs for other `Collider2D`s are welcome!
* when a collider gets fragmented (because of Unity's way of handling colliders), the rope sometimes gets stuck inbetween fragments. There's no work-around here but to make your colliders simpler.
