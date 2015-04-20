# Magic3D
Stupid card game developped for programming skill's improvment. It's far from finished...

Data's come from [Slightly Magic](http://www.slightlymagic.net) and if you've already downloaded card arts, the cached
directory will be shared.

##### Compilation

- init and update submodules
```
cd Magic3D
git submodule init
git submodule update .
```
- build OpenTK first:
```
cd OpenTK
xbuild /p:Configuration=Release OpenTK.sln
cd ..
```
- build Magic3D:
```
xbuild
```
##### Screen shot
![screen shot](/screenshot.png?raw=true "Magic3d")
