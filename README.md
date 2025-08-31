# VolumeTweaker
### A small command line utility to adjust Windows system volume!

- Have .net SDK / Framework 4.8.1 Installed
- Compile project with `dotnet build`
- Find the resulting `VolumeTweaker.exe` in the \bin\Debug\net481

The .exe file can be used directly from a windows command line, or you can use a program which can run it while passing parameters.  

```
.\VolumeTweaker.exe 4.5

.\VolumeTweaker.exe -0.5
```  

Only one parameter can be passed, it must be a float number value between 0 and 100.  That parameter is the percent change that will be applied to the Windows system volume.  The 'Windows on-screen display' for volume will also be triggered to briefly display.  

You can play around with the `System.Threading.Thread.Sleep()` values to change the response times between setting and displaying - it can be a bit fussy to get resulting values that don't wander around.  This method is used because Windows currenty does **not** have an API supported method of triggering the on-screen display of volume, so here it's done with a fake key press event.  

This project started off as a way to make a lightweight background running script that can be run with a Razer brand bluetooth control knob to adjust volume in increments less than Windows standard 2% steps.  In the Razer Synapse software you can make profiles with custom macros attached to the buttons - including doing a 'CMD run' per button press.  VolumeTweaker is lightweight enough that using the control knob to quickly run this as a pulsed command works!  
