# JazzBox
 unity stuff. add it to a project by getting the .git link (click `clone or download` then copy that url) and adding it to the unity package manager (Window > Package Manager > + > Add package from Git URL)

### i can remember what these things do

`billboard.cs` - rotate stuff towards a camera

`blink.cs` - enable/disable a target object on a timer

`colors.cs` - nice color palette

`distorter.cs` - this does a cool thing but i need to check how it works again

`distorttarget.cs` - part of the same thing as above

`fontgrab.cs` - grab random fonts from fonts.google.com

`jazzutil.cs` - currently just SquaredDistance calculation, which is handy

`levelgenerator.cs` - i pulled this from a game i was working on. i think it makes like a vector shaped path or smth

`lisax.cs` - scripting engine. unity implementation of this: https://github.com/jmickle66666666/xlisa_01

`lookat.cs` - rotates object to look at a target

`mouselook.cs` - FPS mouse control

`noisegenerator.cs` - i'm gonna guess it generates noise

`postprocesscamera.cs` - render screen through a material

`quadsplitter.cs` - no idea lol. i think its a procgen thing?

`quantizemesh.cs` - this probably does something cool

`rotate.cs` - rotate stuff over time

`screenshotter.cs` - attach to a camera then u can press P to save a screenshot

`seideldecomposer.cs` - i stole this from somewhere and probably shouldn't have it in this repo but one or more of the procgen things need it

`simpleplayer.cs` - simple FPS movement (make sure to attach mouse looks to object and camera)

`tilecity.cs` - lil level generator thing

### editor stuff

`editor/followscenecamera.cs` - moves the camera to where the scene camera is

`editor/speechgen.cs` - generates text to speech stuff for you (only works on mac because it uses the mac `say` command)

`editor/dog.cs` - i think unity released a dog prefab model on april fool's once. it's pretty useful as a testing model

`editor/autohookpropertydrawer.cs` - very useful thing. please give lotte money https://www.patreon.com/posts/autohook-25908130

`editor/baketexturewindow.cs` - i think i stole this from Ronja, who you should also give money to. https://www.ronja-tutorials.com/

`editor/buildnumber.cs` - every time scripts recompile it'll save a random string to a file to get fake build numbers. might actually be useful in some cases but i just thought it was fun

`editor/texturestomaterials.cs` - right click texture(s) and there's an option to create material(s) for them

### shaders

i need to go through and clean all these up still. mess around with them tho there's cool stuff in there
