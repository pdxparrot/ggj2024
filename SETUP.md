# Setup

## TODO

* Move core scripts into a Core module so they're easier to copy

## Quick Setup

* Copy Managers
* Copy Utils
* Copy Boot.cs
* Update namespaces
* Fix anything that's broken
* Autoload SingletonNodes and SingletonUINodes
  * Any singletons with exported fields needs a scene associated with it
* Setup the boot scene with Boot script on root and make it the main scene
* Setup the client scene and attach it to the boot scene
  * Optionally can also setup the dedicated server scene
* Copy over whatever else seems useful

# Notes

* NPCs are special SimpleCharacter type SimpleNPC
* Players are special SimpleCharacter type SimplePlayer
  * Controlled by player input
