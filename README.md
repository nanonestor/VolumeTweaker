# VolumeTweaker
### A small command line utility to adjust Windows system volume!

- To compile the project yourself - see notes on the github Release page. (It's easy!)

The .exe file can be used directly from a windows command line, or you can use a program which can run it while passing parameters. 

Windows system volume is adjusted by the percent amount passed when the file is run. The passed number must be value between -100 and 100 (It can be a float - i.e. have decimal places).  The Windows OSD 'on-screen display' for volume will also be triggered to briefly display.  The OSD only displays whole number values rounded from the actual value set.

---

Increases the level +12.5%
```
VolumeTweaker.exe 12.5
```
Decreases the level -0.5%
```
VolumeTweaker.exe -0.5
```  

---

This project started off as a way to make a lightweight background running script that can be run with a Razer brand bluetooth control knob to adjust volume in increments less than Windows standard 2% steps.  In the Razer Synapse software you can make profiles with custom macros attached to the buttons - including doing a 'CMD run' per button press.  VolumeTweaker is lightweight enough that using the control knob to quickly run this as a pulsed command works!  


You can play around with the `System.Threading.Thread.Sleep()` values in the pre-compiled code to change the response times between setting and displaying - it can be a bit fussy to get resulting values that don't wander around.  This method is used because Windows currenty does **not** have an API supported method of triggering the on-screen display of volume, so here it's done with a fake key press event.  

---

### Possible argument types

**1.** Adjusts the volume % level from the current setting (-100 to 100)
```
VolumeTweaker.exe -0.25
VolumeTweaker.exe 11.33
```  
**2.** Sends the current system volume % level to console output to two decimal places:
```
VolumeTweaker level
```
**3.** `set` Sets the volume level to the % entered as second argument (0 to 100)
```
VolumeTweaker set 33.25
```
---
