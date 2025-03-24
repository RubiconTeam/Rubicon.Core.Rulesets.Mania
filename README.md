# Rubicon.Core.Rulesets.Mania

An addon for [Rubicon.Core](https://github.com/RubiconTeam/Rubicon.Core) that adds a vertical scrolling rhythm game mode, similar to Stepmania.

Although this add-on will obviously have a bunch of remnants related to Rubicon Engine, this could be used as a base for ones looking to implement a dance-styled rhythm portion into their own game.

## Setting up outside of Rubicon Engine

This is assuming you have already installed these programs and addons:
- [Godot Engine 4.4-stable (Program)](https://godotengine.org/download/archive/4.4-stable/) (No guarantee for older or newer versions.)
- [.NET 8.0 SDK (Program)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PukiTools.GodotSharp (Addon)](https://github.com/Binpuki/PukiTools.GodotSharp)
- [Rubicon.Core (Addon)](https://github.com/RubiconTeam/Rubicon.Core)
### Instructions (With Git) (Recommended)

This method ensures that you can stay up-to-date with whatever branch you need Rubicon.Core on, no matter if it's the latest or another certain version.

1. Make sure you have Godot open along with the .NET SDK installed, and have your project with .NET capabilities open.

2. Navigate to your git project folder in your terminal of choice.

3. Add this repo as a submodule to the Godot Project (Example: `git submodule add https://github.com/RubiconTeam/Rubicon.Core.Rulesets.Mania.git addons/Rubicon.Core.Rulesets.Mania/`) and enable it as a plugin in Project Settings.

4. Click the Build button (the hammer icon on the top right), restart the editor for good measure, and go off!

  

Do keep in mind that each time you clone your project's repo, you will have to run `git submodule init recursive` the first time around, then `git submodule update --remote` to update the submodules.

  

### Instructions (No Git)

This method is the most easiest, but slightly tougher to maintain.

1. Make sure you have Godot open along with the .NET SDK installed, and have your project with .NET capabilities open.

2. Add the contents of this repo as an add-on at path `res://addons/Rubicon.Core.Rulesets.Mania/` and enable the plugin.

3. Click the Build button (the hammer icon on the top right), restart the editor for good measure, and go off!
