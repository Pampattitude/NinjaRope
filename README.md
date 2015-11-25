# `NinjaRope` <sup><sub>v0.2</sub></sup>

Unity3D implementation of the Ninja Rope in Worms Armaggedon.

This implementation has been made for my game, [Scufflers](http://gamejolt.com/games/scufflers-pre-alpha/92083), currently in pre-alpha.

To use, just create a gameobject with a LineRenderer (for the rope rendering) and the script (duh!).
Included is an example player controller to test it out.

C&C and PRs are welcome!

- - - -

## Version history

* v2 (actual): uses raycasting (via `Linecast`)
* v1: uses collision with `Collider2D` for the `NinjaRope`

- - - -

## Known bugs

* the `NinjaHook` makes the target entity twitch when the `NinjaHook` collides with something. See issue #3

- - - -

## Credits

* [danm3d](https://github.com/danm3d): `RequireComponent` of the `LineRenderer` in the `NinjaRope` script ([`#3266e24`](https://github.com/Pampattitude/NinjaRope/commit/3266e24a0993f80931a27554431c7f3598c2e4fd))
* [Lukas182](https://github.com/Lukas182) ([@PixFunStudios](https://twitter.com/PixFunStudios)): twitching upon hook collision with element in `raycast-linecast` branch ([issue `#3`](https://github.com/Pampattitude/NinjaRope/issues/3))
* [Pampattitude](https://twitter.com/Pampattitude): duh!

Thanks for your contribution, PRs are always appreciated!
