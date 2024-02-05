#!/bin/bash

cd src
msbuild -m /p:Configuration=Release
cp bin/Release/API_WE_Mod.dll ../Packing/WorldEditor.dll
cd ..
cd Packing
zip -0 -r WorldEdit.zip *
mv WorldEdit.zip ../../../Mods/WorldEdit.scmod
cd ..