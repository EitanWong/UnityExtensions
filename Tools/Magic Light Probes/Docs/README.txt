                     Magic Light Probes

  What is it?
  -----------

  Magic Light Probes is an editor extension that is designed to help you arrange your light 
  probes in the automatic mode as quickly and correctly as possible.
  
  This tool can guarantee that:
      * No probes will be located inside the geometry, no matter how complex it is, convex 
      or concave. This is very important, because even in the simplest case of manually arranging 
      probes by simple duplication, you will inevitably encounter probes within the geometry.

      * With high accuracy, light probes will be installed at corners and intersections of the 
      geometry. In corners, as a rule, the light intensity is lower, therefore, in order to properly 
      illuminate a dynamic object when approaching such places, it is necessary to install probes there.

      * Depending on the settings, the system will try to place the probes in the most contrasting places.

      * It is guaranteed to be pretty fast


  The Latest Version
  ------------------
    
  The latest version is always available in the asset store. Interim updates 
  not published in the asset store can be obtained by contacting the developer. 
  You can also use the update server (http://motiongamesstudio.com/mlp-update) 
  to receive the latest release, beta, or a single file.
  Use the list of available communication channels in the menu 
  "Tools -> Magic Light Probes -> About MLP..." or at the end of this file

  Documentation
  -------------
    
  Up-to-date documentation is located at
  https://motiongamesstudio.gitbook.io/magic-light-probes/

  CHANGELOG

  v 1.94
  -------------

  Improvements:
    - Added option to disable forced placement of probes at geometry edges.

  Changes:
    - Temporarily disabled dynamic density option for modification.

  v 1.93
  -------------

  Bug Fixes:
    - Fixed rare bug of lost compute shader references
    - Fixed incorrect behavior of the algorithm for removing probes inside the geometry in the engine version 2020.2+

  Changes:
    - Due to the upcoming release of the new asset called Magic Lightmap Switcher, changed the grouping of plug-in objects 
      in the hierarchy, now all added volumes are placed in the Magic Tools -> Magic Light Probes

  v 1.92
  -------------

  Bug Fixes:
    - Minor fixes in MLPForceNoProbes component

  v 1.91
  -------------

  Bug Fixes:
    - MLPForceNoProbes component does not work correctly in some situations

  Improvements:
    - MLPForceNoProbes component can now be used both for a single object and for a group of objects  
    - Corrections have been made that allowed to increase the accuracy of placing probes in the vicinity of objects

  v 1.90
  -------------

  Bug Fixes:
    - Removed console warnings

  v 1.89
  -------------

  Improvements:
    - Added support for Shadowmask modes. Now probes are set differently for Distance Shadowmask and Shadowmask

  Bug Fixes:
    - Completely fixed conflict in Transform extension

  v 1.88
  -------------

  Bug Fixes:
    - Added a namespace to the Trnasform extension as it can cause conflicts with other plugins


  v 1.87
  -------------

  Improvements:
    - Saving custom probe positions even after volume recalculation
    - Improved mechanics for selecting floor and ground objects
    - Optimized scripts (should get rid of lags with a large number of volumes on scene)
    - Disabled notifications about update check errors in the background
    - Probes added manually are now highlighted

  Bug Fixes:
    - Fixed errors for objects with LOD
    - MLP Helper no longer appears when selecting a - MLP Combined Volume -
    - Queue calculation now starts correctly

  Changes:
    - Fixed a typo in the content of the Debug tab      
    - Disabled Volumes Auto-Generation option, as it very often works with errors and requires significant improvement

   v 1.86
  -------------

  Improvements:
    - Ability to consider a group of objects as a single collider (MLPCombinedMesh)

  Bug Fixes:
    - Colliders marked as a trigger are mistakenly taken into account during the calculation
    - MLP Light component does not work correctly with Bakery Light Mesh component
    - The "Corners Detection Threshold" setting in MLP Quick Editing removes probes in contrasting areas

  Changes:
    - Due to the upcoming release of the new asset called Magic Lightmap Switcher, the context 
      menu structure has been changed. Now is Tools -> Magic Tools -> Magic Light Probes
    - Documentation updated

  v 1.85
  -------------

  Bug Fixes:
    - Sometimes an exception is thrown at line 60 in the MLPCombinedVolume.cs file. 
      (The object of type "MLPCombinedVolume" has been destroyed but you are still trying to access it...)        

  v 1.84
  -------------

  Improvements:
    - Added a new component that allows you not to forcibly install probes on the surface of an 
      object to which it is attached (MLPForceNoProbes)

  v 1.83
  -------------

  Bug Fixes:
    - Sometimes an exception is thrown at line 60 in the MLPCombinedVolume.cs file. 
      (The object of type "MLPCombinedVolume" has been destroyed but you are still trying to access it...)  
      NOTICE: Perhaps the error has not been completely fixed. Let me know if you encounter it again.
    - Manual probes placement (Set Custom Probes) is not automatically deactivated and locks 
      the scene view even if the component's inspector is not active
    - Preset of filling options is not taken into account correctly

  Improvements:
    - Parameters ​​in MLP Quick Editing can be entered using the keyboard, not just the sliders
    - Added option to disable automatic shadow border detection (in Simple workflow)

  v 1.82
  -------------

  Bug Fixes:
    - Several errors that did not allow to calculate the volume in the extended mode correctly

  v 1.81
  -------------

  Bug Fixes:
    - Wrong type of light source HDRP
    - Calculation errors in debug mode

  v 1.80
  -------------

  Bug Fixes:
    - A warning window (This object is needed for the plugin to work properly) is displayed while baking lighting with Bakery

  v 1.79
  -------------

  Bug Fixes:
    - Strong editor slowdown with a large number of objects or MLP volumes on scene

  v 1.78
  -------------

  Bug Fixes:
    - Sometimes probes are erroneously removed, even if they are not inside the geometry

  v 1.77
  -------------

  Bug Fixes:
    - Probe counter readings in the main component are fixed

  Changes:
    - The requirement to save the scene before calculation can be turned off

  v 1.75
  -------------

  Improvements:
    - Automatic arrangement of calculation volumes
    - Dynamic Volume Density (Experimental)
    - The requirement to select objects to limit the scene below is now optional
       You can use the bottom side of the volume as a limiter
    - Setting the Gizmo display distance during quick editing

  Bug Fixes:
    - MLP Helper window now displays correctly
    - The console is cleared of warnings

  Changes:
    - Manager window resized
    - An object is created on the scene, into which all the components of the plug-in are added

  v 1.74
  -------------

  Improvements:
    - Tips for options in the editor interface

  Bug Fixes:
    - The warning about the necessary system component is repeated endlessly and blocks the editor 
      if you deactivate the object in which the MLP Combined Volume component is located

  v 1.73
  -------------

  Improvements:
    - Automatically check for updates
    - Links to documentation in system components (help icon in the inspector interface)

  v 1.72
  -------------

  Changes:
    - LoadComputeShaders() method is divided, now the search for the plugin work folder 
    is carried out in a parallel thread, after which the shaders are loaded.

  v 1.71
  -------------

  Bug Fixes:
    - Bugs fixed in debug mode when the "Simple" workflow is selected

  v 1.70
  -------------

  Added features:
    - During quick editing, the volumes that are visible in the editor camera are recalculated first (prioritized view)
    - Scene GUI for adding/removing objects that bound the scene from below (Tools/Magic Light Probes/Scene GUI)
    - Managing a list of objects that bounds the scene from below from the main component interface

  Bug Fixes:
    - Fixed a bug in the inspector of the "MLP Quick Editing" component, which did not allow it to display correctly
    - Errors in the algorithm for arranging probes along geometry are fixed
    - Compute shaders do not work on OpenGL < 4.3 for Windows and Linux. For macOS only Metal Render is supported. Software verification added
    - A method has been found in the search code for probes located below ground/floor that slows down the system if there are many objects on the scene
    - Errors in the "MLP Quick Editing" component - index out of range
    - The wrong plugin’s work path makes it impossible to work system correctly

   v 1.68 (micro patch)
  -------------

  Added features:
    - Added workflow switch - Simple (quick and easy); Advanced (slower and more accurate)

   v 1.67 
  -------------

  Added features:
    - After clicking the "Add Volume ..." button in the manager, a new volume is created in front of the camera

  Bug Fixes:
    - Data for quick editing is saved even after a scene is reloaded

   v 1.66 
  -------------

  Added features:
    - Added the ability to forcibly disable an algorithm that tries to automatically prevent light leaks through walls

  Changes:
    - Added an example of an indoor scene

   v 1.65 
  -------------

  Added features:
    - More visual display of volumes on scene
    - Handles for volume size control
    - Ability to quickly edit the distance between the probes in the corners
    - Cancel button during volumes combining process

  Bug Fixes:
    - In some cases, the probes are erroneously placed below the level of the probe ground/floor
    - The "MLP Quick Editing" component recalculated only for the last part of the total volume
    - Errors in the search algorithm for probes at the range borders of the light source    

  Changes:
    - The option to adjust the distance between the probes in the corners (Corners Probe Spacing) is again available
    - Important documentation changes (be sure to read the Quick Start section before use)

  v 1.6 
  -------------

  Added features:
    - Quick editing of volume parameters after calculation
        * Equivalent Volume Filling (GPU)
        * Unlit Volume Filling (GPU)
        * Color Threshold
    - GPU acceleration for some passes of calculation
    - Display progress in editor status bar

  Bug Fixes:
    - In rare cases, probes were erroneously placed inside the geometry
    - Incorrect partner volume for a light source
    - Probes removed from combined volume after scene reload
    - Incorrect type of light source in HDRP mode

  Changes:
    - Temporarily disabled the ability to set the distance between the probes in the corners

  v 1.5 
  -------------

  Added features:
    - The calculation can now be performed even if the editor window is out of focus 
    - Ability to quickly edit in real-time some parameters of the calculated volume (in the current version it is only "Color Threshold")
    - The light sources in the "Lights" tab of the manager are now sorted by type and mode

  Bug fixes:
    - A pop-up window asking to add plug-in main component to the scene in the prefab editing mode

  Changes:
    - Recalculation of automatic volume separation is started only after the mouse button is released

  v 1.4.3
  -------------

  Added features:
    - HDRP Light Source Support (experimental)  

  v 1.4.2
  -------------

  Added features:
    - Added ability to install probes in manual mode with one click ("MLP Combined Volume" component)

  Bug fixes:
    - The "MagicLightProbes.DependencyChecker.DoesTypeExist()" method throws an exception in some cases, which blocks code compilation
    - The first time you add the "MLP Volume" component to the scene, all non-static objects are deactivated 
    - Minor fixes in DependencyChecker.cs file

  v 1.4.1
  -------------

  Bug fixes:
    - Non-optimal code in the "Find Geometry Edges" pass (with a large number of objects on the scene, the editor freezes for a long time)
  Changes:
    - ETA calculation algorithm is now more accurate

  v 1.4
  -------------

  Added features:
    - Saving a scene before calculation
    - ETA for calculating automatically divided volumes
    - Volume optimization for mixed lighting

  Bug fixes:
    - Terrain irregularities are perceived as geometry edges
    - Colliders are still duplicated in debug mode
	- Geometry intersection detection does not work correctly in some modes
	- On some objects marked as static, colliders are duplicated during the calculation
    - Restoring scene state after calculation in "Debug Mode" works with errors
    - Violation of the queue for calculating volumes (when there are several on the scene)
    - Culling by equivalent color works with errors
    - Wrong progress counter in Removing Unused Points pass
    - The endlessly repeating warning "This component will be automatically disabled when baking." When the main component is disabled in hierarchy
    - Invalid value of probes number counter after calculation
    - Warning in console "Releasing render texture that is set to be RenderTexture.active!"
    - Errors in the console after building the project

  Changes:
	- Minor changes in the interface of the main component

   v 1.3
  -------------

  Added features:
	- Lock settings during calculation
	- Automatic division of large volumes into smaller ones
	- Customizable spacing between probes in corners
	- Removing probes with equivalent color neighbors
	- New component "MLP Force Save Probes" for objects that need to be forced to evenly fill probes
	- Automatic installation of probes at the edges of the geometry

  Bug fixes:
	- Incorrect light source type assignment in "MLP Light" component
	- Scripting Define Symbols are not assigned at the right time (Added ability to force dependency checking)
	- Some disabled objects are not activated after calculation
    - Progress in "Debug Mode" now displays correctly
    - Erroneous message that the light source is outside the calculation volume

  Changes:
	- Manager window moved to Tools -> Magic Light Probes -> MLP Manager
    - Added menu item for forced dependency checking Tools -> Magic Light Probes -> Check Dependencies
    - Added menu item Tools -> Magic Light Probes -> About MLP...

   v 1.2 
  -------------

  Added features:
	- Bakery GPU Lightmapper integration (support all light types)
	- Probes optimization after volume combination
	- More visual display of Gizmo for some light sources
	- Highlighting console MLP messages
	- Detailed progress bar in the manager
	- New "Global Settings" tab in the manager (editing general settings for all volumes)

  Bug fixes:
	- Invalid demo shader
	- Fixed bugs in probes placement algorithms
	- Fixed a bug due to which the baking involved probes that should have been disabled

  Changes:
	- Manager window moved to Windows -> MLP Manager

  v 1.1.0.1 
  -------------
  
  Bug fixes:
	- Uncontrolled duplication of an object "-- MLP Combined Volume --" in 2019.2 or newer

   v 1.1 
  -------------

  Added features:
	- Duplication to a specified height with a given step
	- The calculation mode "One by one" (for a large number of volumes on the scene)
	- Progress bars in the manager window

  Bug fixes:
	- The manager window does not update the list of volumes when the volume is duplicated on the scene
	- Rare cases of light probes placement far beyond the volume
	- Double light probes near geometry
	- Uncontrolled duplication of an object "-- MLP Combined Volume --"
	- Invalid shader for displaying light probe positions in debug mode
	- Exception during scene unloading
	- Error restoring scene state after calculation

  v 1.0
  -------------

  First release.

  Contacts
  -------------

  e-mail: evb45@bk.ru
  telegram: https://t.me/EVB45
  forum: https://forum.unity.com/threads/magic-light-probes-quickly-build-high-precision-volumes-of-light-probes.779309/
  discord channel: https://discord.gg/p94azzE