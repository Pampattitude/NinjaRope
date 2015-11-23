> As of 2015-11-23, **the `master` branch is deprecated**. Use the `raycast-linecast` branch for more accurate results &mdash; uses raycasting instead of rectangular colliders.
>
> Known `raycast-linecast` branch issues:
> * [#3 Twitching upon hook collision with element](https://github.com/Pampattitude/NinjaRope/issues/3)
> * [#5 Comments](https://github.com/Pampattitude/NinjaRope/issues/5)
>
> [Here](https://www.dropbox.com/s/dagw7t8wlx2sx66/NinjaRope%20-%20Test%20Scene.zip?dl=0) is a testable Unity3D scene

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

- - - -

## Credits

* [danm3d](https://github.com/danm3d): `RequireComponent` of the `LineRenderer` in the `NinjaRope` script ([`#3266e24`](https://github.com/Pampattitude/NinjaRope/commit/3266e24a0993f80931a27554431c7f3598c2e4fd))

Thanks for your contribution, PRs are always appreciated!
