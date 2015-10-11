# NinjaRope
Unity3D implementation of the Ninja Rope in Worms Armaggedon.

This implementation has been made for my game, [Scufflers](http://gamejolt.com/games/scufflers-pre-alpha/92083), currently in pre-alpha.

To use, just create a gameobject with a LineRenderer (for the rope rendering) and the script (duh!).
Included is an example player controller to test it out.

C&C and PRs are welcome!

- - - -

## Known bugs

* The `NinjaRopePlayerController` gets twitchy when the hook collides with an obstacle (and calls the `onHooked()` delegate)

- - - -

## Credits

* [danm3d](https://github.com/danm3d): `RequireComponent` of the `LineRenderer` in the `NinjaRope` script ([`#3266e24`](https://github.com/Pampattitude/NinjaRope/commit/3266e24a0993f80931a27554431c7f3598c2e4fd))

Thanks for your contribution, PRs are always appreciated!
