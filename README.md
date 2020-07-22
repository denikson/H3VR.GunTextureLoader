# H3VR.GunTextureLoader

An **experimental example plugin** that shows how to load and replace assets at runtime.

**WIP:** Only replaces textures, no builds available at the moment.

## Requirements

* [BepInEx 5.2 or newer](https://github.com/BepInEx/BepInEx)
* [XUnity.ResourceRedirector 4.12.0 or newer](https://github.com/bbepis/XUnity.AutoTranslator) (comes as part of XUnity.AutoTranslator)

## Installation

1. Install dependencies above
2. Copy required DLLs into `lib` folder (check the README in said folder)
3. Build the project
4. Place the built `H3VR.GunTextureLoader.dll` (located in `bin` folder) into `BepInEx/plugins` folder


## Usage

* On first run, plugin creates `GunSkins` folder (configurable via plugin's config or [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager))
* Place textures you want to replace as `.png` files into said folder
    - Subfolders are allowed
* The name of the texture must match the name of it in the prefab/asset bundle
    - You can find out the name by opening the asset bundles with UABE or UtinyRipper
	- Alternatively, in `BepInEx/config/BepInEx.cfg`, find and set
	    
	    ```ini
		[Logging.Disk]
		
		LogLevels = All
		```
		
	    Run the game and spawn the gun you want. The names of the textures will be logged into `BepInEx/LogOutput.log`
* Right now only texture replacement is available, but it is possible to replace entire prefabs using [resource redirection](https://github.com/bbepis/XUnity.AutoTranslator#implementing-a-resource-redirector).