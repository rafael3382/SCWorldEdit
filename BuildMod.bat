cd src
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" /m /p:Configuration=Release
move bin\Release\API_WE_Mod.dll ..\Packing\WorldEditor.dll
cd ..

cd Packing
tar.exe -a -c -f WorldEdit.zip *
move WorldEdit.zip %userprofile%\OneDrive\Documentos\SCApi\1.44\Mods\WorldEdit.scmod
%userprofile%\OneDrive\Documentos\SCApi\1.44\Survivalcraft.exe
cd ..